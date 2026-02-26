using Microsoft.AspNetCore.Mvc;
using SpiEyes.DAL;
using SpiEyes.Models;

namespace SpiEyes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PushNotificationController : ControllerBase
{
    private DatabaseContext _databaseContext { get; set; }

    public PushNotificationController(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }

    [HttpPost("SaveSubscription")]
    public async Task<IActionResult> SaveSubscription([FromBody] PushSubscription pushSubscription)
    {
        try
        {
            pushSubscription.Id = Guid.NewGuid();
            await _databaseContext.AddAsync(pushSubscription);
            await _databaseContext.SaveChangesAsync();
            return new CreatedResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR | [{nameof(PushNotificationController)} -> {nameof(SaveSubscription)}] {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}