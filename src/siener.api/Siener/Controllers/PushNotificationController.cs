using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Siener.DAL;
using Siener.Models;
using WebPush;

namespace Siener.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PushNotificationController : ControllerBase
{
    private readonly Config _config;
    private DatabaseContext _databaseContext { get; set; }

    public PushNotificationController(IOptions<Config> configOptions, DatabaseContext databaseContext)
    {
        _config = configOptions.Value;
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

    [HttpPost("SendNotification")]
    public async Task<IActionResult> SendNotification()
    {
        var webPushClient = new WebPushClient();

        try
        {
            var ps = _databaseContext.PushSubscriptions.Include(x => x.Keys).FirstOrDefault();
            
            var subscription = new WebPush.PushSubscription(ps.Endpoint, ps.Keys.P256dh, ps.Keys.Auth);
            var vapidDetails = new VapidDetails($"mailto: {_config.Email}", _config.VapidKeys.Public, _config.VapidKeys.Private);

            await webPushClient.SendNotificationAsync(subscription, "payload", vapidDetails);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR | [{nameof(PushNotificationController)} -> {nameof(SaveSubscription)}] {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}