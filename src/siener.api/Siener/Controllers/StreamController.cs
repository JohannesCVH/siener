using Microsoft.AspNetCore.Mvc;
using Siener.Services;
using Siener.Utility;

namespace Siener.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StreamController : ControllerBase
{   
    public ISharedDataService _sharedDataService { get; set; }
    private readonly CameraService _cameraService;
    
    public StreamController(ISharedDataService sharedDataService)
    {
        _sharedDataService = sharedDataService;
    }

    [HttpGet("{cameraName}")]
    public async Task<IActionResult> Stream(string cameraName)
    {
        try
        {
            var path = PathUtils.CreateAppPath($"Streams/{cameraName}/output.m3u8");
            return PhysicalFile(path, "application/x-mpegURL");
        }
        catch(Exception ex)
        {
            Console.WriteLine($"ERROR: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError, new { message= "An unexpected error occurred." });
        }
    }

    [HttpGet("{cameraName}/{segmentId}.ts")]
    public IActionResult GetStreamSegment(string cameraName, string segmentId)
    {
        var filePath = PathUtils.CreateAppPath($"Streams/{cameraName}/Segments/{segmentId}.ts");
        return PhysicalFile(filePath, "video/MP2T");
    }

    [HttpGet("Test")]
    public IActionResult Test(CancellationToken cancellationToken)
    {
        _cameraService.StopAsync(cancellationToken);
        
        return Ok();
    }
}