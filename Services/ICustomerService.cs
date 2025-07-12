using OfferManagement.API.DTOs;

namespace OfferManagement.API.Services;

public interface ICustomerService
{
    Task<List<CustomerDto>> GetCustomersByCompanyAsync(int companyId);
    Task<CustomerDto?> GetCustomerByIdAsync(int id, int companyId);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request, int companyId);
    Task<CustomerDto?> UpdateCustomerAsync(int id, UpdateCustomerRequest request, int companyId);
    Task<bool> DeleteCustomerAsync(int id, int companyId);
}
