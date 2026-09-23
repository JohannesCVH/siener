using System.Threading.Channels;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Siener.Data;
using Siener.Data.Entities;
using Siener.Models;
using Siener.Services;
using Siener.Utility;
using static Siener.Utility.LoggerExtensions;

public class EventBackgroundService : IHostedService
{
    private readonly Config _config;
    private readonly ISharedDataService _sharedDataService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IObjectDetectionService _objectDetectionService;
    private record FrameProcessingRequest(Camera Camera, string FilePath);
    private readonly List<ChannelWriter<FrameProcessingRequest>> _channelWriters = new();
    private readonly ILogger<EventBackgroundService> _logger;

    private static readonly Dictionary<string, DetectionTypes> LabelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "person", DetectionTypes.Person },
        { "dog",    DetectionTypes.Dog },
        { "car",    DetectionTypes.Car }
    };
    
    public EventBackgroundService(
        IOptions<Config> configOptions,
        ISharedDataService sharedDataService, 
        IServiceScopeFactory scopeFactory, 
        IObjectDetectionService objectDetectionService,
        ILogger<EventBackgroundService> logger
    )
    {
        _config = configOptions.Value;
        _sharedDataService = sharedDataService;
        _scopeFactory = scopeFactory;
        _objectDetectionService = objectDetectionService;
        _logger = logger;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var camera in _sharedDataService.Cameras!)
        {
            var channel = Channel.CreateUnbounded<FrameProcessingRequest>();
            _channelWriters.Add(channel.Writer);

            _ = Task.Run(() => ConsumeCameraFramesAsync(channel.Reader, cancellationToken));
            
            camera.FrameWatcher = new FileSystemWatcher(camera.FramePath);
            camera.FrameWatcher.Filter = "*.jpg";
            camera.FrameWatcher.Created += async (s,e) =>
            {
                channel.Writer.TryWrite(new FrameProcessingRequest(camera, e.FullPath));
            };
            camera.FrameWatcher.EnableRaisingEvents = true;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var channel in _channelWriters)
        {
            channel.TryComplete();
        }

        return Task.CompletedTask;
    }

    private async Task ConsumeCameraFramesAsync(ChannelReader<FrameProcessingRequest> reader, CancellationToken cancellationToken)
    {
        await foreach (var request in reader.ReadAllAsync(cancellationToken))
        {
            await ProcessFrameAsync(request.Camera, request.FilePath, cancellationToken);
        }
    }

    private async Task ProcessFrameAsync(Camera camera, string filePath, CancellationToken cancellationToken)
    {
        string methodName = nameof(ProcessFrameAsync);
        
        try
        {
            byte[]? buffer = null;
            int retryCount = 0;
            do
            {
                if (retryCount >= 3)
                    break;
                
                if (retryCount > 0)
                    _logger.LogMessage(LogType.Information, methodName, $"Image read retry: {retryCount}");

                await Task.Delay(100 << retryCount); //Delay initially so file has time to be written to disk fully.
                
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    buffer = new byte[fs.Length];
                    await fs.ReadExactlyAsync(buffer, 0, buffer.Length, cancellationToken);
                };

                retryCount++;
            }
            while (buffer.Length == 0);

            if (buffer is null || buffer.Length == 0)
            {
                _logger.LogMessage(LogType.Error, methodName, "Buffer cannot be empty.");
                return;
            }

            var detections = await _objectDetectionService.DetectAsync(camera, buffer);

            await ProcessEventAsync(camera, detections, cancellationToken);
        }
        catch(Exception ex)
        {
            _logger.LogMessage(LogType.Error, methodName, ex.Message);
        }
    }

    private async Task ProcessEventAsync(Camera camera, IEnumerable<ObjectDetectionResponse> detections, CancellationToken cancellationToken)
    {
        string methodName = nameof(ProcessEventAsync);
        
        short detectedFlags = 0;

        foreach (var detection in detections)
        {
            if (LabelMap.TryGetValue(detection.Label, out var type) && detection.Confidence > 0.65)
                detectedFlags |= (short)type;
        }

        if (detectedFlags == (short)DetectionTypes.None || detectedFlags == (short)DetectionTypes.Car)
        {
            await EndEventAsync(camera, false, cancellationToken);

            return;
        }

        // Console.WriteLine($"Detected Flags: {detectedFlags}");
        await StartOrUpdateEventAsync(camera, detectedFlags, cancellationToken);
    }

    private async Task EndEventAsync(Camera camera, bool endImmediate, CancellationToken cancellationToken)
    {
        string methodName = nameof(EndEventAsync);
        
        try
        {
            using (IServiceScope scope = _scopeFactory.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var detectionEvent = await dbContext.Events.Where(x => x.Camera == camera.Name).FirstOrDefaultAsync(x => x.EndTime == null, cancellationToken);
                if (detectionEvent is null)
                    return;


                camera.EventEndFrameCount++;
                _logger.LogMessage(LogType.Information, methodName, $"[Camera -> {camera.Name}] Event end frame count: {camera.EventEndFrameCount}", new Dictionary<string, string>() { { "Camera", camera.Name } });
                
                if (camera.EventEndFrameCount > 4)
                {
                    detectionEvent.EndTime = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
            }

            camera.EventEndFrameCount = 0;
            _logger.LogMessage(LogType.Information, methodName, $"[Camera -> {camera.Name}] Event ended", new Dictionary<string, string>() { { "Camera", camera.Name } });
        }
        catch(Exception ex)
        {
            _logger.LogMessage(LogType.Error, methodName, ex.Message);
        }
    }

    private async Task StartOrUpdateEventAsync(Camera camera, short detectedFlags, CancellationToken cancellationToken)
    {        
        string methodName = nameof(StartOrUpdateEventAsync);
        
        try
        {
            using (IServiceScope scope = _scopeFactory.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                bool shouldAdd = false;
                var currentTime = DateTime.UtcNow;

                var detectionEvent = await dbContext.Events.Where(x => x.Camera == camera.Name).FirstOrDefaultAsync(x => x.EndTime == null);
                if (detectionEvent is not null && detectionEvent.SessionId != _config.SessionId)
                {
                    await EndEventAsync(camera, true, cancellationToken);
                    detectionEvent = null;
                }
                
                if (detectionEvent is null)
                {
                    shouldAdd = true;
                    detectionEvent = new Event
                    {
                        SessionId = _config.SessionId,
                        Camera = camera.Name,
                        StartTime = currentTime,
                        Notified = false
                    };
                }

                detectionEvent.DetectionTypes |= detectedFlags;

                if (shouldAdd)
                {
                    _logger.LogMessage(LogType.Information, methodName, $"[Camera -> {camera.Name}] Event started");
                    await dbContext.AddAsync(detectionEvent);

                    if (!string.IsNullOrEmpty(_sharedDataService.FcmToken))
                    {   
                        var message = new Message
                        {
                            Token = _sharedDataService.FcmToken,
                            Notification = new Notification
                            {
                                Title = $"Detection Event on {camera}",
                                Body = $"Detected: {((DetectionTypes)detectedFlags).ToString()}"
                            }
                        };

                        string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                        _logger.LogMessage(LogType.Information, methodName, response);
                    }
                }

                await dbContext.SaveChangesAsync();
            }
        }
        catch(Exception ex)
        {
            _logger.LogMessage(LogType.Error, methodName, ex.Message);
        }
    }
}