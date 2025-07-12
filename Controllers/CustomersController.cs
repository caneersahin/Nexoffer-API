using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfferManagement.API.DTOs;
using OfferManagement.API.Services;

namespace OfferManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomers()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var customers = await _customerService.GetCustomersByCompanyAsync(companyId);
        return Ok(customers);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetCustomer(int id)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var customer = await _customerService.GetCustomerByIdAsync(id, companyId);
        if (customer == null)
        {
            return NotFound("Customer not found");
        }

        return Ok(customer);
    }

    [HttpPost]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var customer = await _customerService.CreateCustomerAsync(request, companyId);
        return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerRequest request)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var customer = await _customerService.UpdateCustomerAsync(id, request, companyId);
        if (customer == null)
        {
            return NotFound("Customer not found");
        }

        return Ok(customer);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomer(int id)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var success = await _customerService.DeleteCustomerAsync(id, companyId);
        if (!success)
        {
            return NotFound("Customer not found");
        }

        return Ok(new { Success = true, Message = "Customer deleted successfully" });
    }
}
