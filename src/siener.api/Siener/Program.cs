using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Sinks.OpenSearch;
using Siener.Data;
using Siener.Models;
using Siener.Services;
using YoloDotNet;
using YoloDotNet.ExecutionProvider.Cpu;
using YoloDotNet.Models;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(name: "AllowAll", policy =>
            {
                policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
            });
        });

        builder.Configuration.AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".siener/config.json"), optional: false, reloadOnChange: true);
        builder.Services.Configure<Config>(builder.Configuration.GetSection("Configuration"));
        builder.Services.PostConfigure<Config>(config =>
        {
           config.SessionId = Guid.NewGuid(); 
        });

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(connectionString));

        var modelPath = builder.Configuration.GetSection("Configuration")["OnnxLocation"];
        var yolo = new Yolo(new YoloOptions
        {
            ExecutionProvider = new CpuExecutionProvider(modelPath!)
        });

        builder.Services.AddSingleton(yolo);
        builder.Services.AddSingleton<IObjectDetectionService, ObjectDetectionService>();

        builder.Services.AddSingleton<ISharedDataService, SharedDataService>();
        builder.Services.AddSingleton<MediaMtxService>();
        builder.Services.AddSingleton<FFmpegService>();
        builder.Services.AddHostedService<CameraService>();
        builder.Services.AddHostedService<FileCleanupService>();
        builder.Services.AddHostedService<EventBackgroundService>();

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .WriteTo.OpenSearch(new OpenSearchSinkOptions(new Uri(builder.Configuration.GetSection("Configuration")["OpenSearchUrl"]!))
            {
                AutoRegisterTemplate = true,
                IndexFormat = $"{builder.Environment.ApplicationName.ToLower().Replace(".", "-")}-logs-{{0:yyyy.MM.dd}}",
                NumberOfReplicas = 0,
                NumberOfShards = 1
            })
            .CreateLogger();

        builder.Host.UseSerilog();

        var app = builder.Build();
        app.MapControllers();
        app.UseCors("AllowAll");
        app.UseSerilogRequestLogging();

        // using (var scope = app.Services.CreateScope())
        // {
        //     var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

        //     try 
        //     {
        //         // 1. Verify connection
        //         if (db.Database.CanConnect())
        //         {
        //             Console.WriteLine("Successfully connected to the database.");

        //             // 2. Test access to the events table
        //             var eventCount = db.Events.Count();
        //             Console.WriteLine($"Successfully accessed 'events' table. Total count: {eventCount}");
        //         }
        //         else
        //         {
        //             Console.WriteLine("Failed to connect to the database.");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         // Log the error
        //         Console.WriteLine($"An error occurred while testing the database: {ex.Message}");
        //     }
        // }

        app.Run();
    }
}