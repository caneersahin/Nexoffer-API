using Microsoft.EntityFrameworkCore;
using OfferManagement.API.Data;
using OfferManagement.API.DTOs;
using OfferManagement.API.Models;

namespace OfferManagement.API.Services;

public class CompanyService : ICompanyService
{
    private readonly ApplicationDbContext _context;
    private readonly IWebHostEnvironment _environment;

    public CompanyService(ApplicationDbContext context, IWebHostEnvironment environment)
    {
        _context = context;
        _environment = environment;
    }

    public async Task<CompanyDto?> GetCompanyByIdAsync(int id)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return null;

        return MapToDto(company);
    }

    public async Task<CompanyDto?> GetCompanyByUserIdAsync(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.CompanyId == null) return null;

        var company = await _context.Companies.FindAsync(user.CompanyId);
        if (company == null) return null;

        return MapToDto(company);
    }

    public async Task<CompanyDto?> UpdateCompanyAsync(int id, UpdateCompanyRequest request)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return null;

        company.Name = request.Name;
        company.Address = request.Address;
        company.Phone = request.Phone;
        company.Email = request.Email;
        company.TaxNumber = request.TaxNumber;
        company.IBAN = request.IBAN;
        company.Website = request.Website;

        await _context.SaveChangesAsync();
        return MapToDto(company);
    }

    public async Task<bool> UploadLogoAsync(int id, IFormFile logo)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return false;

        var rootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(rootPath))
        {
            rootPath = Path.Combine(_environment.ContentRootPath, "wwwroot");
        }

        var uploadsFolder = Path.Combine(rootPath, "uploads", "logos");
        Directory.CreateDirectory(uploadsFolder);

        var fileName = $"{company.Id}_{Guid.NewGuid()}{Path.GetExtension(logo.FileName)}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await logo.CopyToAsync(fileStream);
        }

        company.Logo = $"/uploads/logos/{fileName}";
        await _context.SaveChangesAsync();

        return true;
    }

    private static CompanyDto MapToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            Logo = company.Logo,
            Address = company.Address,
            Phone = company.Phone,
            Email = company.Email,
            TaxNumber = company.TaxNumber,
            IBAN = company.IBAN,
            Website = company.Website,
            SubscriptionPlan = company.SubscriptionPlan.ToString(),
            SubscriptionStartDate = company.SubscriptionStartDate,
            SubscriptionEndDate = company.SubscriptionEndDate,
            OffersUsed = company.OffersUsed,
            IsActive = company.IsActive
        };
    }

    public async Task<CompanyDto?> UpgradePlanAsync(int id, UpgradePlanRequest request)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return null;

        var payment = new Payment
        {
            Amount = request.Amount,
            TransactionId = request.TransactionId,
            CompanyId = id
        };

        _context.Payments.Add(payment);

        company.SubscriptionPlan = request.Plan;
        company.SubscriptionStartDate = payment.PaidAt;
        company.SubscriptionEndDate = payment.PaidAt.AddMonths(1);
        company.IsActive = true;

        await _context.SaveChangesAsync();

        return MapToDto(company);
    }

    public async Task<PaymentDto?> RecordPaymentAsync(int id, RecordPaymentRequest request)
    {
        var company = await _context.Companies.FindAsync(id);
        if (company == null) return null;

        var payment = new Payment
        {
            Amount = request.Amount,
            TransactionId = request.TransactionId,
            CompanyId = id
        };

        _context.Payments.Add(payment);

        company.SubscriptionStartDate = payment.PaidAt;
        company.SubscriptionEndDate = payment.PaidAt.AddMonths(1);
        company.IsActive = true;

        await _context.SaveChangesAsync();

        return new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            PaidAt = payment.PaidAt,
            TransactionId = payment.TransactionId
        };
    }

    public async Task<List<PaymentDto>> GetPaymentHistoryAsync(int id)
    {
        var payments = await _context.Payments
            .Where(p => p.CompanyId == id)
            .OrderByDescending(p => p.PaidAt)
            .ToListAsync();

        return payments.Select(p => new PaymentDto
        {
            Id = p.Id,
            Amount = p.Amount,
            PaidAt = p.PaidAt,
            TransactionId = p.TransactionId
        }).ToList();
    }
}