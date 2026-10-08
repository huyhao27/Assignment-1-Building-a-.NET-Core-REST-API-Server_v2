using GameInventoryApi.Models;
using GameInventoryApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GameInventoryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Player")]
public class PlayerProfileController : ControllerBase
{
    private readonly IPlayerProfileService _service;

    public PlayerProfileController(IPlayerProfileService service) => _service = service;

    // Only allow player to access their own profile
    [HttpGet]
    public async Task<ActionResult<PlayerProfile>> GetMyProfile()
    {
        var playerId = User.FindFirst("PlayerId")?.Value;
        if (playerId == null) return Unauthorized();

        var profile = await _service.GetByFilterAsync(p => p.PlayerId == playerId);
        if (profile is null) return NotFound();
        return profile;
    }

    [HttpPut]
    public async Task<ActionResult> UpdateMyProfile([FromBody] PlayerProfile profile)
    {
        var playerId = User.FindFirst("PlayerId")?.Value;
        if (playerId == null || profile.PlayerId != playerId) return Forbid();

        await _service.UpdateAsync(profile.Id, profile);
        return NoContent();
    }
}
