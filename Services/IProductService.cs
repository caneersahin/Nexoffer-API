using OfferManagement.API.DTOs;

namespace OfferManagement.API.Services;

public interface IProductService
{
    Task<List<ProductDto>> GetProductsByCompanyAsync(int companyId);
    Task<ProductDto?> GetProductByIdAsync(int id, int companyId);
    Task<ProductDto> CreateProductAsync(CreateProductRequest request, int companyId);
    Task<ProductDto?> UpdateProductAsync(int id, UpdateProductRequest request, int companyId);
    Task<bool> DeleteProductAsync(int id, int companyId);
}
