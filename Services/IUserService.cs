using OfferManagement.API.DTOs;

namespace OfferManagement.API.Services;

public interface IUserService
{
    Task<List<UserDto>> GetUsersByCompanyAsync(int companyId);
    Task<UserDto?> CreateUserAsync(CreateUserRequest request, int companyId);
    Task<UserDto?> UpdateUserAsync(string id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(string id);
    Task<bool> ToggleUserStatusAsync(string id);
}