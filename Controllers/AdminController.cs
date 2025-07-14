using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfferManagement.API.DTOs;
using OfferManagement.API.Services;

namespace OfferManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
//[Authorize(Roles = "SuperAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Ok(dashboard);
    }

    [HttpPost("companies/{id}/upgrade")]
    public async Task<IActionResult> UpgradeCompany(int id, [FromBody] UpgradePlanRequest request)
    {
        var company = await _adminService.UpgradeCompanyAsync(id, request);
        if (company == null)
        {
            return NotFound("Company not found");
        }
        return Ok(company);
    }
}
