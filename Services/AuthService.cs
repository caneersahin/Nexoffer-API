using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OfferManagement.API.Data;
using OfferManagement.API.DTOs;
using OfferManagement.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OfferManagement.API.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return new AuthResponse { Success = false, Message = "Invalid credentials" };
        }

        if (!user.IsActive)
        {
            return new AuthResponse { Success = false, Message = "Account is deactivated" };
        }

        var token = await GenerateJwtTokenAsync(user);
        
        return new AuthResponse
        {
            Success = true,
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                CompanyId = user.CompanyId
            }
        };
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
        {
            return new AuthResponse { Success = false, Message = "Email already exists" };
        }

        // Create company first
        var company = new Company
        {
            Name = request.CompanyName,
            Address = request.CompanyAddress,
            Phone = request.CompanyPhone,
            Email = request.Email
        };

        _context.Companies.Add(company);
        await _context.SaveChangesAsync();

        // Create user
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CompanyId = company.Id
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthResponse
            {
                Success = false,
                Message = string.Join(", ", result.Errors.Select(e => e.Description))
            };
        }

        await _userManager.AddToRoleAsync(user, "Admin");

        var token = await GenerateJwtTokenAsync(user);
        
        return new AuthResponse
        {
            Success = true,
            Token = token,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                CompanyId = user.CompanyId
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
    {
        // Basic implementation - in production, use proper refresh token logic
        var principal = GetPrincipalFromExpiredToken(request.Token);
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        
        if (userId == null)
        {
            return new AuthResponse { Success = false, Message = "Invalid token" };
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new AuthResponse { Success = false, Message = "User not found" };
        }

        var newToken = await GenerateJwtTokenAsync(user);
        
        return new AuthResponse
        {
            Success = true,
            Token = newToken,
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                CompanyId = user.CompanyId
            }
        };
    }

    public async Task<bool> LogoutAsync(string userId)
    {
        // In a real implementation, you would invalidate the refresh token
        return await Task.FromResult(true);
    }

    private async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
            new("CompanyId", user.CompanyId?.ToString() ?? "")
        };

        var roles = await _userManager.GetRolesAsync(user);
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]!)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
        
        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}