using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpiEyes.DAL;
using SpiEyes.Models;

namespace SpiEyes.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private DatabaseContext _databaseContext { get; set; }
    
    public UserController(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
    }
    
    [HttpPost("Save")]
    public async Task<IActionResult> Save([FromBody] User user)
    {
        try
        {
            await _databaseContext.AddAsync(user);
            await _databaseContext.SaveChangesAsync();
            return new CreatedResult();
        }
        catch(Exception ex)
        {
            Console.WriteLine($"ERROR | [{nameof(UserController)} -> Save] {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        try
        {
            var user = await _databaseContext.Users.Where(x => x.Username == loginRequest.Username).FirstOrDefaultAsync();
            if (user is null)
                return new UnauthorizedResult();
            
            return new OkObjectResult(user);
        }
        catch(Exception ex)
        {
            Console.WriteLine($"ERROR | [{nameof(UserController)} -> Login] {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}