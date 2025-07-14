using Microsoft.EntityFrameworkCore;
using OfferManagement.API.Data;
using OfferManagement.API.DTOs;
using OfferManagement.API.Models;

namespace OfferManagement.API.Services;

public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;

    public ProductService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductDto>> GetProductsByCompanyAsync(int companyId)
    {
        var products = await _context.Products
            .Where(p => p.CompanyId == companyId)
            .ToListAsync();

        return products.Select(MapToDto).ToList();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, int companyId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        return product == null ? null : MapToDto(product);
    }

    public async Task<ProductDto?> CreateProductAsync(CreateProductRequest request, int companyId)
    {
        // İsim kontrolü (null, boşluk vs)
        if (string.IsNullOrWhiteSpace(request.Name))
            return null;

        // Aynı isimde ürün var mı kontrolü (aynı şirket içinde)
        var existingProduct = await _context.Products
            .FirstOrDefaultAsync(p => p.Name == request.Name && p.CompanyId == companyId);

        if (existingProduct != null)
            return null;

        var product = new Product
        {
            Name = request.Name,
            Description = request.Description,
            Category = request.Category,
            Price = request.Price,
            CompanyId = companyId
        };

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return MapToDto(product);
    }


    public async Task<ProductDto?> UpdateProductAsync(int id, UpdateProductRequest request, int companyId)
    {
        // İsim kontrolü
        if (string.IsNullOrWhiteSpace(request.Name))
            return null;

        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        if (product == null)
            return null;

        // Aynı isimde başka bir ürün var mı kontrolü (aynı ID'de olmayan)
        var duplicate = await _context.Products
            .AnyAsync(p => p.Id != id && p.CompanyId == companyId && p.Name == request.Name);

        if (duplicate)
            return null;

        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.Price = request.Price;

        await _context.SaveChangesAsync();

        return MapToDto(product);
    }


    public async Task<bool> DeleteProductAsync(int id, int companyId)
    {
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.CompanyId == companyId);

        if (product == null) return false;

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return true;
    }

    private static ProductDto MapToDto(Product product)
    {
        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Category = product.Category,
            Price = product.Price
        };
    }
}
