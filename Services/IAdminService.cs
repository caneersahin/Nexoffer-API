using OfferManagement.API.DTOs;

namespace OfferManagement.API.Services;

public interface IAdminService
{
    Task<AdminDashboardDto> GetDashboardAsync();
    Task<CompanyDto?> UpgradeCompanyAsync(int companyId, UpgradePlanRequest request);
}
