using Siener.Lib.Models;
using Siener.ML.Services;
using YoloDotNet;
using YoloDotNet.ExecutionProvider.Cpu;
using YoloDotNet.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".siener/config.json"), optional: false, reloadOnChange: true);
builder.Services.Configure<Config>(builder.Configuration.GetSection("Configuration"));

var modelPath = builder.Configuration.GetSection("Configuration")["OnnxLocation"];
var yolo = new Yolo(new YoloOptions
{
    ExecutionProvider = new CpuExecutionProvider(modelPath)
});

builder.Services.AddSingleton(yolo);
builder.Services.AddScoped<IObjectDetectionService, ObjectDetectionService>();
builder.Services.AddControllers();

var app = builder.Build();
app.MapControllers();
app.Run();