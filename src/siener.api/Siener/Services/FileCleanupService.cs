using Microsoft.Extensions.Options;
using Siener.Models;
using Siener.Services;

public class FileCleanupService : BackgroundService
{
    public IOptions<Config> _config { get; set; }
    private readonly ISharedDataService _sharedDataService;
    
    public FileCleanupService(IOptions<Config> config, ISharedDataService sharedDataService)
    {
        _config = config;
        _sharedDataService = sharedDataService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromSeconds(10));

        while (!cancellationToken.IsCancellationRequested && await timer.WaitForNextTickAsync(cancellationToken))
        {
            if (_sharedDataService.Cameras is null)
                continue;

            foreach (var camera in _sharedDataService.Cameras)
            {
                var files = Directory.GetFiles(camera.FramePath);
                var cutoffTime = DateTime.Now.AddSeconds(-10);
                foreach (var file in files)
                {
                    var fileInfo = new FileInfo(file);
                    if (fileInfo.LastWriteTime < cutoffTime)
                        File.Delete(file);
                }
            }
        }
    }
}