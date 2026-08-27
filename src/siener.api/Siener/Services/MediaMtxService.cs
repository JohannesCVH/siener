using System.Diagnostics;
using Microsoft.Extensions.Options;
using Siener.Models;
using Siener.Utility;

namespace Siener.Services;

public class MediaMtxService
{
    private readonly Config _config;
    private readonly ILogger<EventBackgroundService> _logger;

    public MediaMtxService(
        IOptions<Config> configOptions,
        ILogger<EventBackgroundService> logger
    )
    {
        _config = configOptions.Value;
        _logger = logger;
    }

    public async Task<Process> StartProcessAsync()
    {
        var frameStartInfo = new ProcessStartInfo
        {
            FileName = _config.MediaMtxLocation,
            Arguments = "",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = frameStartInfo };
        process.Start();
        int retryCount = 0;
        while (!await HealthCheckAsync())
        {
            Console.WriteLine("[MediaMtxService -> StartProcessAsync] Health check failed, delaying and retrying.");
            if (retryCount > 2)
            {
                var exMsg = "MediaMTX health check failed.";
                throw new Exception(exMsg);
            }
            await Task.Delay(2000 << retryCount);
            retryCount++;
        }

        // _ = Task.Run(() => ReadOutput(_mediaMtxProc));

        return process;
    }

    public async Task<bool> HealthCheckAsync()
    {
        using var client = new HttpClient();
        var apiUrl = $"http://localhost:9997/v3/info";
        try
        {
            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode) return true;
            else return false;
        }
        catch (Exception ex)
        {
            //TODO: Log Error
            return false;
        }
    }

    public async Task AddCamera(CameraConfig cameraConfig)
    {
        string methodName = nameof(AddCamera);
        
        var camerasPath = PathUtils.GetCamerasPath();
        
        using var client = new HttpClient();
        var apiUrl = $"http://localhost:9997/v3/config/paths/add/{cameraConfig.Name}";

        var config = new
        {
            source = cameraConfig.URL + "/stream1",
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
            _logger.LogMessage(LogType.Error, methodName, "Couldn't push camera to MediaMTX.", new Dictionary<string, string>(){ { "Exception", ex.Message } });
        }
    }

    // private void ReadOutput(Process proc)
    // {
    //     string line;
    //     while ((line = proc.StandardOutput.ReadLine()) != null)
    //     {   
    //         Console.WriteLine($"[MediaMTX] | " + line);
    //     }
    // }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopping MediaMTX");
    }
}