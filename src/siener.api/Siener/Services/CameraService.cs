using System.Diagnostics;
using Microsoft.Extensions.Options;
using Siener.Lib.Models;
using Siener.Models;
using Siener.Utility;

namespace Siener.Services;

public class CameraService : IHostedService
{
    private readonly Config _config;
    private readonly ISharedDataService _sharedDataService;
    public List<Camera>? Cameras { get; set; }

    private static bool LOG_FRAMES = false;
    
    public CameraService(IOptions<Config> configOptions, ISharedDataService sharedDataService, IHostApplicationLifetime appLifetime)
    {
        _config = configOptions.Value;
        _sharedDataService = sharedDataService;
        DirectoryUtils.GenerateCameraFolders(_config.Cameras);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Cameras = new List<Camera>();
        
        for (int i = 0; i < _config.Cameras.Length; i++)
        {
            var camerasPath = PathUtils.GetCamerasPath();
        
            var frameStartInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments =
                    $"-rtsp_transport tcp -fflags nobuffer -flags low_delay -i \"{_config.Cameras[i].URL}\" -vf fps=1 {camerasPath}/{_config.Cameras[i].Name}/Frames/frame_%04d.jpg",
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
                recordPath = $"{camerasPath}/%path/Recordings/%Y-%m-%d_%H-%M-%S-%f",
                recordSegmentDuration = "4s",
                recordPartDuration = "1s",
                recordDeleteAfter = "30s"
            };

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsJsonAsync(apiUrl, config);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR | [{nameof(CameraService)}: {_config.Cameras[i].Name}] | Couldn't push camera to MediaMTX. Exception: {ex.Message}");
                continue;
            }

            var camera = new Camera
            {
                Name = _config.Cameras[i].Name,
                FrameProc = frameProc
            };
            var camPath = PathUtils.GetCameraPath(camera.Name);
            camera.FramePath = Path.Combine(camPath, "Frames");
            Cameras.Add(camera);
        }

        _sharedDataService.Cameras = Cameras;

        // foreach (Camera cam in Cameras)
        // {
        //     _ = Task.Run(() => ReadErrorFrame(cam));
        // }
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

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopping FFmpeg processes");
    }
}