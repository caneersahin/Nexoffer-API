using OfferManagement.API.DTOs;
using OfferManagement.API.Models;

namespace OfferManagement.API.Services;

public interface ICompanyService
{
    Task<CompanyDto?> GetCompanyByIdAsync(int id);
    Task<CompanyDto?> GetCompanyByUserIdAsync(string userId);
    Task<CompanyDto?> UpdateCompanyAsync(int id, UpdateCompanyRequest request);
    Task<bool> UploadLogoAsync(int id, IFormFile logo);
    Task<CompanyDto?> UpgradePlanAsync(int id, UpgradePlanRequest request);
    Task<PaymentDto?> RecordPaymentAsync(int id, RecordPaymentRequest request);
    Task<List<PaymentDto>> GetPaymentHistoryAsync(int id);
}