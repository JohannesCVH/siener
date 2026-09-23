using System.Diagnostics;
using Siener.Models;
using Siener.Utility;
using SkiaSharp;
using YoloDotNet;

namespace Siener.Services;

public interface IObjectDetectionService
{
    public Task<IEnumerable<ObjectDetectionResponse>> DetectAsync(Camera camera, byte[] buffer);
}

public class ObjectDetectionService : IObjectDetectionService
{
    private readonly Yolo _yoloEngine;
    private readonly ILogger<ObjectDetectionService> _logger;
    
    public ObjectDetectionService(
        Yolo yoloEngine,
        ILogger<ObjectDetectionService> logger
    )
    {
        _yoloEngine = yoloEngine;
        _logger = logger;
    }

    public async Task<IEnumerable<ObjectDetectionResponse>> DetectAsync(Camera camera, byte[] buffer)
    {
        string methodName = nameof(DetectAsync);
        
        try
        {
            var sw = new Stopwatch();
            sw.Start();
            
            using var bitmap = SKBitmap.Decode(buffer);
            if (bitmap == null) throw new ArgumentException("Invalid image stream.");
            var results = _yoloEngine.RunObjectDetection(bitmap).Select(r => new ObjectDetectionResponse{
                Label = r.Label.Name,
                Confidence = r.Confidence
            });

            _logger.LogMessage(LogType.Information, methodName, $"Object detection service responded in: {sw.ElapsedMilliseconds}ms for camera: {camera}");

            _logger.LogMessage(LogType.Information, methodName, $"[Objects Detected] -> Amount: {results.Count()}", new Dictionary<string, string>() { { "Camera", camera.Name } });
            foreach (var detection in results)
            {
                if (detection.Label == "person" || detection.Label == "dog")
                {
                    _logger.LogMessage(LogType.Information, methodName, $"[Object detected] -> Label: {detection.Label}, Confidence: {detection.Confidence}", new Dictionary<string, string>() { 
                        { "Camera", camera.Name },
                        { "Label", detection.Label },
                        { "Confidence", detection.Confidence.ToString() }
                    });
                }
            }

            return results;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
        }
        
        return [];
    }
}