using Siener.Lib.Models;
using SkiaSharp;
using YoloDotNet;

namespace Siener.ML.Services;

public interface IObjectDetectionService
{
    public Task<IEnumerable<ObjectDetectionResponse>> DetectAsync(Stream imgStream, CancellationToken cancellationToken);
}

public class ObjectDetectionService : IObjectDetectionService
{
    private readonly Yolo _yoloEngine;
    
    public ObjectDetectionService(Yolo yoloEngine)
    {
        _yoloEngine = yoloEngine;
    }

    public async Task<IEnumerable<ObjectDetectionResponse>> DetectAsync(Stream imgStream, CancellationToken cancellationToken)
    {
        try
        {
            using var bitmap = SKBitmap.Decode(imgStream);
            if (bitmap == null) throw new ArgumentException("Invalid image stream.");
            var results = _yoloEngine.RunObjectDetection(bitmap).Select(r => new ObjectDetectionResponse{
                Label = r.Label.Name,
                Confidence = r.Confidence
            });

            return results;
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[ObjectDetectionService -> DetectAsync] | ERROR: {ex.Message}");
        }
        
        return [];
    }
}