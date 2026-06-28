using System.Diagnostics;
using Siener.Lib.Models;
using Siener.Services;

public class EventBackgroundService : IHostedService
{
    private ISharedDataService _sharedDataService;
    private HttpClient _httpClient;
    
    public EventBackgroundService(ISharedDataService sharedDataService)
    {
        _sharedDataService = sharedDataService;
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri("http://localhost:49991");
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var camera in _sharedDataService.Cameras)
        {
            camera.FrameWatcher = new FileSystemWatcher(camera.FramePath);
            camera.FrameWatcher.Filter = "*.jpg";
            camera.FrameWatcher.Created += async (s,e) => await ProcessFrameAsync(camera.Name, e.FullPath);
            camera.FrameWatcher.EnableRaisingEvents = true;
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private async Task ProcessFrameAsync(string camera, string filePath)
    {
        Console.WriteLine($"Camera: {camera}, File: {filePath}");
        try
        {
            byte[] fileBytes;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                fileBytes = new byte[fs.Length];
                await fs.ReadAsync(fileBytes, 0, fileBytes.Length);
            };

            var content = new ByteArrayContent(fileBytes);
            var sw = new Stopwatch();
            sw.Start();
            var response = await _httpClient.PostAsync("api/ObjectDetection/Detect", content);

            if (response.IsSuccessStatusCode)
            {
                var detections = await response.Content.ReadFromJsonAsync<IEnumerable<ObjectDetectionResponse>>();
                Console.WriteLine($"Object detection api responded in: {sw.ElapsedMilliseconds}ms");
                File.Delete(filePath);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"[EventBackgroundService -> ProcessFrameAsync] | ERROR: {ex.Message}");
        }
    }
}