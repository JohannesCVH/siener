using System.Diagnostics;
using System.Threading.Channels;
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
    private record FrameProcessingRequest(string Camera, string FilePath);
    private readonly List<ChannelWriter<FrameProcessingRequest>> _channelWriters = new();
    private readonly ILogger<EventBackgroundService> _logger;

    private static readonly Dictionary<string, DetectionTypes> LabelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "person", DetectionTypes.Person },
        { "dog", DetectionTypes.Dog },
        { "car", DetectionTypes.Car }
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
            camera.FrameWatcher.Created += async (s,e) => channel.Writer.TryWrite(new FrameProcessingRequest(camera.Name, e.FullPath));
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

    private async Task ProcessFrameAsync(string camera, string filePath, CancellationToken cancellationToken)
    {
        string methodName = nameof(ProcessEventAsync);
        
        // Console.WriteLine($"Camera: {camera}, File: {filePath}");
        try
        {
            byte[] buffer;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                buffer = new byte[fs.Length];
                await fs.ReadAsync(buffer, 0, buffer.Length);
            };

            if (buffer.Length == 0)
            {
                _logger.LogMessage(LogType.Error, methodName, "Buffer cannot be empty.");
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            var detections = await _objectDetectionService.DetectAsync(buffer);
            await ProcessEventAsync(camera, detections, cancellationToken);

            _logger.LogMessage(LogType.Information, methodName, $"Object detection api responded in: {sw.ElapsedMilliseconds}ms");
        }
        catch(Exception ex)
        {
            _logger.LogMessage(LogType.Error, methodName, ex.Message);
        }
    }

    private async Task ProcessEventAsync(string camera, IEnumerable<ObjectDetectionResponse> detections, CancellationToken cancellationToken)
    {
        short detectedFlags = 0;

        foreach (var detection in detections)
        {
            if (LabelMap.TryGetValue(detection.Label, out var type))
                detectedFlags |= (short)type;
        }


        if (detectedFlags == (short)DetectionTypes.None)
        {
            await EndEventAsync(camera, cancellationToken);
            return;
        }

        // Console.WriteLine($"Detected Flags: {detectedFlags}");
        await StartOrUpdateEventAsync(camera, detectedFlags, cancellationToken);
    }

    private async Task EndEventAsync(string camera, CancellationToken cancellationToken)
    {
        string methodName = nameof(EndEventAsync);
        
        try
        {
            using (IServiceScope scope = _scopeFactory.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                var detectionEvent = await dbContext.Events.Where(x => x.Camera == camera).FirstOrDefaultAsync(x => x.EndTime == null, cancellationToken);
                if (detectionEvent is null)
                    return;

                detectionEvent.EndTime = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);
            }

            _logger.LogMessage(LogType.Information, methodName, $"Ended event for {camera}");
        }
        catch(Exception ex)
        {
            _logger.LogMessage(LogType.Error, methodName, ex.Message);
        }
    }

    private async Task StartOrUpdateEventAsync(string camera, short detectedFlags, CancellationToken cancellationToken)
    {        
        string methodName = nameof(StartOrUpdateEventAsync);
        
        try
        {
            using (IServiceScope scope = _scopeFactory.CreateAsyncScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

                bool shouldAdd = false;
                var currentTime = DateTime.UtcNow;

                var detectionEvent = await dbContext.Events.Where(x => x.Camera == camera).FirstOrDefaultAsync(x => x.EndTime == null);
                if (detectionEvent is not null && detectionEvent.SessionId != _config.SessionId)
                {
                    await EndEventAsync(camera, cancellationToken);
                    detectionEvent = null;
                }
                
                if (detectionEvent is null)
                {
                    shouldAdd = true;
                    detectionEvent = new Event
                    {
                        SessionId = _config.SessionId,
                        Camera = camera,
                        StartTime = currentTime,
                        Notified = false
                    };
                }

                detectionEvent.DetectionTypes |= detectedFlags;

                if (shouldAdd)
                {
                    _logger.LogMessage(LogType.Information, methodName, "Detection event started");
                    await dbContext.AddAsync(detectionEvent);
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