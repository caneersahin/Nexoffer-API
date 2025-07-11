using OfferManagement.API.DTOs;

namespace OfferManagement.API.Services;

public interface IOfferService
{
    Task<OfferDto?> CreateOfferAsync(CreateOfferRequest request, string userId);
    Task<OfferDto?> GetOfferByIdAsync(int id, string userId);
    Task<List<OfferDto>> GetOffersByUserAsync(string userId, int page = 1, int pageSize = 10);
    Task<OfferDto?> UpdateOfferAsync(int id, UpdateOfferRequest request, string userId);
    Task<bool> DeleteOfferAsync(int id, string userId);
    Task<bool> SendOfferAsync(int id, string userId);
}