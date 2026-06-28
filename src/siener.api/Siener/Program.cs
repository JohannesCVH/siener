using Microsoft.EntityFrameworkCore;
using Siener.DAL;
using Siener.Lib.Models;
using Siener.Services;

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

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        builder.Services.AddDbContext<DatabaseContext>(options => options.UseNpgsql(connectionString));

        builder.Services.AddSingleton<ISharedDataService, SharedDataService>();
        builder.Services.AddHostedService<MediaMtxService>();
        builder.Services.AddHostedService<CameraService>();
        builder.Services.AddHostedService<FileCleanupService>();

        var app = builder.Build();
        app.MapControllers();
        app.UseCors("AllowAll");

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            db.Database.EnsureCreated();
            DbInitializer.Initialize(db);
        }

        app.Run();
    }
}