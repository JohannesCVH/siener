using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using Siener.Data;
using Siener.Data.Entities;
using Siener.Models;
using Siener.Services;

public class EventBackgroundService : IHostedService
{
    private readonly ISharedDataService _sharedDataService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IObjectDetectionService _objectDetectionService;
    private record FrameProcessingRequest(string Camera, string FilePath);
    private readonly List<ChannelWriter<FrameProcessingRequest>> _channelWriters = new();

    private static readonly Dictionary<string, DetectionTypes> LabelMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "person", DetectionTypes.Person },
        { "dog", DetectionTypes.Dog },
        { "car", DetectionTypes.Car }
    };
    
    public EventBackgroundService(ISharedDataService sharedDataService, IServiceScopeFactory scopeFactory, IObjectDetectionService objectDetectionService)
    {
        _sharedDataService = sharedDataService;
        _scopeFactory = scopeFactory;
        _objectDetectionService = objectDetectionService;
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var camera in _sharedDataService.Cameras)
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
        Console.WriteLine($"Camera: {camera}, File: {filePath}");
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
                Console.WriteLine($"[EventBackgroundService -> ProcessFrameAsync] | ERROR: buffer cannot be empty.");
                return;
            }

            var sw = new Stopwatch();
            sw.Start();
            var detections = await _objectDetectionService.DetectAsync(buffer);
            await ProcessEventAsync(camera, detections, cancellationToken);

            Console.WriteLine($"Object detection api responded in: {sw.ElapsedMilliseconds}ms");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[EventBackgroundService -> ProcessFrameAsync] | ERROR: {ex.Message}");
        }
        finally
        {
            File.Delete(filePath);
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
        using (IServiceScope scope = _scopeFactory.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            var detectionEvent = await dbContext.Events.Where(x => x.Camera == camera).FirstOrDefaultAsync(x => x.EndTime == null, cancellationToken);
            if (detectionEvent is null)
                return;

            detectionEvent.EndTime = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        Console.WriteLine($"[Event] | Ended event for {camera} at {DateTime.Now}");
    }

    private async Task StartOrUpdateEventAsync(string camera, short detectedFlags, CancellationToken cancellationToken)
    {
        using (IServiceScope scope = _scopeFactory.CreateAsyncScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

            bool shouldAdd = false;

            var detectionEvent = await dbContext.Events.Where(x => x.Camera == camera).FirstOrDefaultAsync(x => x.EndTime == null);
            if (detectionEvent is null)
            {
                shouldAdd = true;
                detectionEvent = new Event
                {
                    Camera = camera,
                    StartTime = DateTime.UtcNow,
                    Notified = false
                };
            }

            detectionEvent.DetectionTypes |= detectedFlags;

            if (shouldAdd)
            {
                await dbContext.AddAsync(detectionEvent);
                Console.WriteLine($"[Event] | Added event for {camera} at {DateTime.Now}");
            }
            else
            {
                Console.WriteLine($"[Event] | Updated event for {camera} at {DateTime.Now}");
            }

            await dbContext.SaveChangesAsync();
        }

        
    }
}