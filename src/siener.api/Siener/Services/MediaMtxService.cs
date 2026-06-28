using System.Diagnostics;
using Microsoft.Extensions.Options;
using Siener.Lib.Models;

namespace Siener.Services;

public class MediaMtxService : IHostedService
{
    private readonly Config _config;

    public MediaMtxService(IOptions<Config> configOptions)
    {
        _config = configOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
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

        var mediaMtxProc = new Process { StartInfo = frameStartInfo };
        mediaMtxProc.Start();

        _ = Task.Run(() => ReadOutput(mediaMtxProc));
    }

    private void ReadOutput(Process proc)
    {
        string line;
        while ((line = proc.StandardOutput.ReadLine()) != null)
        {   
            Console.WriteLine($"[MediaMTX] | " + line);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Console.WriteLine("Stopping MediaMTX");
    }
}