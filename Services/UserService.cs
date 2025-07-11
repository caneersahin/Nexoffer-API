using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OfferManagement.API.Data;
using OfferManagement.API.DTOs;
using OfferManagement.API.Models;

namespace OfferManagement.API.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserService(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<List<UserDto>> GetUsersByCompanyAsync(int companyId)
    {
        var users = await _context.Users
            .Where(u => u.CompanyId == companyId)
            .Select(u => new UserDto
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email!,
                CompanyId = u.CompanyId,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return users;
    }

    public async Task<UserDto?> CreateUserAsync(CreateUserRequest request, int companyId)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null) return null;

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyId = companyId
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded) return null;

        await _userManager.AddToRoleAsync(user, "User");

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CompanyId = user.CompanyId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserDto?> UpdateUserAsync(string id, UpdateUserRequest request)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return null;

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.Email = request.Email;
        user.UserName = request.Email;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded) return null;

        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            CompanyId = user.CompanyId,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);
        return result.Succeeded;
    }

    public async Task<bool> ToggleUserStatusAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return false;

        user.IsActive = !user.IsActive;
        var result = await _userManager.UpdateAsync(user);
        return result.Succeeded;
    }
}