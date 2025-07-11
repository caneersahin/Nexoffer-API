using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfferManagement.API.DTOs;
using OfferManagement.API.Services;

namespace OfferManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var users = await _userService.GetUsersByCompanyAsync(companyId);
        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var user = await _userService.CreateUserAsync(request, companyId);
        if (user == null)
        {
            return BadRequest("Failed to create user");
        }

        return CreatedAtAction(nameof(GetUsers), user);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var user = await _userService.UpdateUserAsync(id, request);
        if (user == null)
        {
            return NotFound("User not found");
        }

        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var success = await _userService.DeleteUserAsync(id);
        if (!success)
        {
            return NotFound("User not found");
        }

        return Ok(new { Success = true, Message = "User deleted successfully" });
    }

    [HttpPost("{id}/toggle-status")]
    public async Task<IActionResult> ToggleUserStatus(string id)
    {
        var success = await _userService.ToggleUserStatusAsync(id);
        if (!success)
        {
            return NotFound("User not found");
        }

        return Ok(new { Success = true, Message = "User status updated successfully" });
    }
}