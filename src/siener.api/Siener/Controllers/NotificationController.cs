using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Siener.Models;
using Siener.Services;
using WebPush;

namespace Siener.Controllers;

public record TokenDto(string Token);

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly Config _config;
    private readonly ISharedDataService _sharedDataService;

    public NotificationController(
        IOptions<Config> configOptions,
        ISharedDataService sharedDataService
    )
    {
        _config = configOptions.Value;
        _sharedDataService = sharedDataService;
    }

    [HttpPost("RegisterToken")]
    public async Task<IActionResult> RegisterToken([FromBody] TokenDto tokenDto)
    {
        try
        {
            _sharedDataService.FcmToken = tokenDto.Token;
            
            return new OkResult();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR | [{nameof(NotificationController)} -> {nameof(RegisterToken)}] {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("SendNotification")]
    public async Task<IActionResult> SendNotification()
    {
        var webPushClient = new WebPushClient();

        try
        {
            var message = new Message
            {
                Token = _sharedDataService.FcmToken,
                Notification = new Notification
                {
                    Title = $"Detection Event on Driveway"
                },
            };

            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR | [{nameof(NotificationController)} -> {nameof(SendNotification)}] {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}