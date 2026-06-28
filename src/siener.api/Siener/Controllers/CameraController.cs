using Microsoft.AspNetCore.Mvc;
using Siener.Models;
using Siener.Services;
using Siener.Utility;

namespace Siener.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraController : ControllerBase
    {
        public ISharedDataService _sharedDataService { get; set; }

        public CameraController(ISharedDataService sharedDataService)
        {
            _sharedDataService = sharedDataService;
        }

        [HttpGet("GetThumbnail/{cameraName}")]
        public IActionResult GetThumbnail(string cameraName)
        {
            var path = PathUtils.GetAppPath($"Streams/{cameraName}/Frames");
            IEnumerable<string> images = Directory.EnumerateFiles(path);
            var thumb = images.MaxBy(x => int.Parse(string.Concat(x.Where(char.IsDigit))));
            var filePath = Path.Combine(path, thumb);
            return PhysicalFile(filePath, "image/jpeg");
        }

        [HttpGet("Streams")]
        public IActionResult Streams()
        {
            var cams = _sharedDataService.Cameras.Select(x => new CameraDto{ Name = x.Name }).ToList();
            return new OkObjectResult(cams);
        }
    }
}