using System.Diagnostics;
using Siener.Models;
using Siener.Utility;

namespace Siener.Services;

public class FFmpegService
{
    public FFmpegService()
    {
        
    }

    public async Task<Process> StartProcessAsync(CameraConfig cameraConfig)
    {
        var camerasPath = PathUtils.GetCamerasPath();
        
        var procStartInfo = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments =
                $"-skip_frame nokey -rtsp_transport tcp -fflags nobuffer -flags low_delay -i \"{cameraConfig.URL}/stream1\" -vf fps=1/4 {camerasPath}/{cameraConfig.Name}/Frames/frame_%04d.jpg",
                // $"-rtsp_transport tcp -fflags nobuffer -flags low_delay -i \"{cameraConfig.URL}/stream2\" -vf fps=1/4 {camerasPath}/{cameraConfig.Name}/Frames/frame_%04d.jpg",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = procStartInfo };
        process.Start();

        return process;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopping FFmpeg");
    }
}