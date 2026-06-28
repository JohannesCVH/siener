using Microsoft.AspNetCore.Mvc;
using Siener.ML.Services;

namespace Siener.ML.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ObjectDetectionController : ControllerBase
{
    private readonly IObjectDetectionService _objectDetectionService;
    
    public ObjectDetectionController(IObjectDetectionService objectDetectionService)
    {
        _objectDetectionService = objectDetectionService;
    }

    [HttpPost("Detect")]
    public async Task<IActionResult> DetectAsync(CancellationToken cancellationToken)
    {
        using var mStream = new MemoryStream();
        await Request.Body.CopyToAsync(mStream);
        mStream.Position = 0;
        var res = await _objectDetectionService.DetectAsync(mStream, cancellationToken);
        
        return new OkObjectResult(res);
    }
}