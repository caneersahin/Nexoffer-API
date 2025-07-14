using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfferManagement.API.DTOs;
using OfferManagement.API.Services;

namespace OfferManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CompanyController : ControllerBase
{
    private readonly ICompanyService _companyService;

    public CompanyController(ICompanyService companyService)
    {
        _companyService = companyService;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMyCompany()
    {
        var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return BadRequest("Invalid user");
        }

        var company = await _companyService.GetCompanyByUserIdAsync(userId);
        if (company == null)
        {
            return NotFound("Company not found");
        }

        return Ok(company);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateCompany([FromBody] UpdateCompanyRequest request)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var company = await _companyService.UpdateCompanyAsync(companyId, request);
        if (company == null)
        {
            return NotFound("Company not found");
        }

        return Ok(company);
    }

    [HttpPost("logo")]
    public async Task<IActionResult> UploadLogo([FromForm] IFormFile logo)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        if (logo == null || logo.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        var success = await _companyService.UploadLogoAsync(companyId, logo);
        if (!success)
        {
            return BadRequest("Failed to upload logo");
        }

        return Ok(new { Success = true, Message = "Logo uploaded successfully" });
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> UpgradePlan([FromBody] UpgradePlanRequest request)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var company = await _companyService.UpgradePlanAsync(companyId, request);
        if (company == null)
        {
            return NotFound("Company not found");
        }

        return Ok(company);
    }

    [HttpPost("payment")]
    public async Task<IActionResult> RecordPayment([FromBody] RecordPaymentRequest request)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var payment = await _companyService.RecordPaymentAsync(companyId, request);
        if (payment == null)
        {
            return NotFound("Company not found");
        }

        return Ok(payment);
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentHistory()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var payments = await _companyService.GetPaymentHistoryAsync(companyId);
        return Ok(payments);
    }
}