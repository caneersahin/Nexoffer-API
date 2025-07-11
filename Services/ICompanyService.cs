using OfferManagement.API.DTOs;

namespace OfferManagement.API.Services;

public interface ICompanyService
{
    Task<CompanyDto?> GetCompanyByIdAsync(int id);
    Task<CompanyDto?> GetCompanyByUserIdAsync(string userId);
    Task<CompanyDto?> UpdateCompanyAsync(int id, UpdateCompanyRequest request);
    Task<bool> UploadLogoAsync(int id, IFormFile logo);
}