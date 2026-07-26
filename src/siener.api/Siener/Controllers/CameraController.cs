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
        public async Task<IActionResult> GetThumbnailAsync(string cameraName)
        {
            var cameraPath = PathUtils.GetCameraPath(cameraName, "Frames");
            IEnumerable<string> images = Enumerable.Empty<string>();
            
            images = Directory.EnumerateFiles(cameraPath);
            if (!images.Any()) return new NoContentResult();
            
            var thumb = images.MaxBy(x =>
            {
                var fileName = Path.GetFileName(x);
                var chars = fileName.Where(char.IsDigit);
                int num = int.Parse(string.Concat(chars));
                return num;
            });
            if (thumb is null) throw new Exception("Thumb cannot be empty.");
            var filePath = Path.Combine(cameraPath, thumb);
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