using OfferManagement.API.DTOs;

namespace OfferManagement.API.Services;

public interface IOfferService
{
    Task<OfferOperationResponse> CreateOfferAsync(CreateOfferRequest request, string userId);
    Task<OfferDto?> GetOfferByIdAsync(int id, int companyId);
    Task<List<OfferDto>> GetOffersByUserAsync(string userId, int page = 1, int pageSize = 10);
    Task<List<OfferDto>> GetOffersByCompanyAsync(int companyId, int page = 1, int pageSize = 10);
    Task<OfferDto?> UpdateOfferAsync(int id, UpdateOfferRequest request);
    Task<bool> DeleteOfferAsync(int id, string userId);
    Task<bool> SendOfferAsync(int id, string userId);
}