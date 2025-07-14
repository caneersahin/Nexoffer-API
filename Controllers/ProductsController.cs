using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfferManagement.API.DTOs;
using OfferManagement.API.Services;

namespace OfferManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var products = await _productService.GetProductsByCompanyAsync(companyId);
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var product = await _productService.GetProductByIdAsync(id, companyId);
        if (product == null)
        {
            return NotFound("Product not found");
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var product = await _productService.CreateProductAsync(request, companyId);

        if (product == null)
        {
            return BadRequest("Ürün adı boş olamaz veya aynı isimde ürün zaten mevcut.");
        }

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductRequest request)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var product = await _productService.UpdateProductAsync(id, request, companyId);
        if (product == null)
        {
            // Belki loglamak için servisten bool + hata mesajı da döndürülebilir
            return BadRequest("Ürün bulunamadı veya geçersiz veri gönderildi.");
        }

        return Ok(product);
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var companyIdClaim = User.FindFirst("CompanyId")?.Value;
        if (companyIdClaim == null || !int.TryParse(companyIdClaim, out var companyId))
        {
            return BadRequest("Invalid company");
        }

        var success = await _productService.DeleteProductAsync(id, companyId);
        if (!success)
        {
            return NotFound("Product not found");
        }

        return Ok(new { Success = true, Message = "Product deleted successfully" });
    }
}
