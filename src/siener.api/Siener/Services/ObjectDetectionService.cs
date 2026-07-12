using Siener.Models;
using SkiaSharp;
using YoloDotNet;

namespace Siener.Services;

public interface IObjectDetectionService
{
    public Task<IEnumerable<ObjectDetectionResponse>> DetectAsync(byte[] buffer);
}

public class ObjectDetectionService : IObjectDetectionService
{
    private readonly Yolo _yoloEngine;
    
    public ObjectDetectionService(Yolo yoloEngine)
    {
        _yoloEngine = yoloEngine;
    }

    public async Task<IEnumerable<ObjectDetectionResponse>> DetectAsync(byte[] buffer)
    {
        try
        {
            using var bitmap = SKBitmap.Decode(buffer);
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