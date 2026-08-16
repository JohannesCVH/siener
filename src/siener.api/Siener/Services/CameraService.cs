using System.Diagnostics;
using Microsoft.Extensions.Options;
using Siener.Models;
using Siener.Utility;

namespace Siener.Services;

public class CameraService : IHostedService
{
    private readonly Config _config;
    private readonly ISharedDataService _sharedDataService;
    private readonly FFmpegService _ffmpegService;
    private readonly MediaMtxService _mediaMtxService;
    public List<Camera>? Cameras { get; set; }
    private Process _mediaMtxProc { get; set; }

    private static bool LOG_FRAMES = false;
    
    public CameraService(
        IOptions<Config> configOptions, 
        ISharedDataService sharedDataService,
        FFmpegService ffmpegService,
        MediaMtxService mediaMtxService
    )
    {
        _config = configOptions.Value;
        _sharedDataService = sharedDataService;
        _ffmpegService = ffmpegService;
        _mediaMtxService = mediaMtxService;
        DirectoryUtils.GenerateCameraFolders(_config.Cameras);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        Cameras = new List<Camera>();
        _mediaMtxProc = await _mediaMtxService.StartProcessAsync();
        
        for (int i = 0; i < _config.Cameras.Length; i++)
        {
            var camera = new Camera()
            {
                Name = _config.Cameras[i].Name,
                FrameProc = await _ffmpegService.StartProcessAsync(_config.Cameras[i])
            };

            await _mediaMtxService.AddCamera(_config.Cameras[i]);

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