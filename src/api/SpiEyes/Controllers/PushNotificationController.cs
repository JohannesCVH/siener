using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
    public async Task<IActionResult> SaveSubscription([FromBody] Models.PushSubscription pushSubscription)
    {
        try
        {
            var existing = await _databaseContext.PushSubscriptions.Include(x => x.Keys).FirstOrDefaultAsync(x => x.UserID == pushSubscription.UserID);
            if (existing != null)
            {
                existing.Endpoint = pushSubscription.Endpoint;
                existing.Keys.P256dh = pushSubscription.Keys.P256dh;
                existing.Keys.Auth = pushSubscription.Keys.Auth;
                _databaseContext.PushSubscriptions.Update(existing);
            }
            else
        {
            pushSubscription.Id = Guid.NewGuid();
            await _databaseContext.AddAsync(pushSubscription);
            }
            
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