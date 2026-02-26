using System.Diagnostics;
using Microsoft.Extensions.Options;
using SpiEyes.Models;
using SpiEyes.Utility;

namespace SpiEyes.Services;

public class CameraService : IHostedService
{
    private readonly Config _config;
    private readonly ISharedDataService _sharedDataService;
    public List<Camera>? Cameras { get; set; }

    private static bool LOG_FRAMES = false;
    
    public CameraService(IOptions<Config> configOptions, ISharedDataService sharedDataService)
    {
        _config = configOptions.Value;
        _sharedDataService = sharedDataService;
        DirectoryUtils.GenerateStreamFolders(_config.Cameras);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Cameras = new List<Camera>();
        
        for (int i = 0; i < _config.Cameras.Length; i++)
        {
            var streamsPath = PathUtils.CreateStreamsPath();
        
            var frameStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments =
                    $"-rtsp_transport tcp -fflags nobuffer -flags low_delay -i \"{_config.Cameras[i].URL}\" -vf fps=1 {streamsPath}/{_config.Cameras[i].Name}/Frames/frame_%04d.jpg",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var frameProc = new Process { StartInfo = frameStartInfo };
            frameProc.Start();

            using var client = new HttpClient();
            var apiUrl = $"http://localhost:9997/v3/config/paths/add/{_config.Cameras[i].Name}";

            var config = new
            {
                source = _config.Cameras[i].URL,
                sourceOnDemand = false,
                rtspTransport = "tcp",
                record = true,
                recordFormat = "fmp4",
                recordPath = $"{streamsPath}/%path/Recordings/%Y-%m-%d_%H-%M-%S-%f",
                recordSegmentDuration = "1m",
                recordPartDuration = "1s",
            };

            var camera = new Camera();

            var response = await client.PostAsJsonAsync(apiUrl, config);
            if (response.IsSuccessStatusCode)
                Console.WriteLine($"ERROR | [{nameof(CameraService)}: {_config.Cameras[i].Name}] | Couldn't push camera to MediaMTX.");

            camera.Name = _config.Cameras[i].Name;
            camera.FrameProc = frameProc;
            Cameras.Add(camera);
        }

        _sharedDataService.Cameras = Cameras;

        foreach (Camera cam in Cameras)
        {
            _ = Task.Run(() => ReadErrorFrame(cam));
        }
    }

    private void ReadErrorFrame(Camera cam)
    {
        string line;
        while ((line = cam.FrameProc.StandardError.ReadLine()) != null)
        {
            if (line.Contains("configuration:")) continue;
            if (!LOG_FRAMES && line.Contains("frame=")) continue;
            
            Console.WriteLine($"[Camera: {cam.Name}] [Frame] [FFmpeg]\t" + line);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (Cameras != null)
        {
            foreach(Camera cam in Cameras)
            {
                cam.StreamProc.Kill();
                cam.StreamProc.WaitForExit(3000);
                cam.StreamProc.Dispose();
            }
        }

        return Task.CompletedTask;
    }
}