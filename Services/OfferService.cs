using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using OfferManagement.API.Data;
using OfferManagement.API.DTOs;
using OfferManagement.API.Models;
using System.ComponentModel.Design;

namespace OfferManagement.API.Services;

public class OfferService : IOfferService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;

    public OfferService(ApplicationDbContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    public async Task<OfferOperationResponse> CreateOfferAsync(CreateOfferRequest request, string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user?.CompanyId == null)
        {
            return new OfferOperationResponse { Success = false, Message = "Invalid user" };
        }

        var company = await _context.Companies.FirstOrDefaultAsync(c => c.Id == user.CompanyId.Value);
        if (company == null)
        {
            return new OfferOperationResponse { Success = false, Message = "Company not found" };
        }

        if (company.SubscriptionEndDate.HasValue && company.SubscriptionEndDate.Value < DateTime.UtcNow)
        {
            company.IsActive = false;
            await _context.SaveChangesAsync();
            return new OfferOperationResponse { Success = false, Message = "Subscription expired. Please renew to continue." };
        }

        if (company.SubscriptionPlan == SubscriptionPlan.Free && company.OffersUsed >= 3)
        {
            return new OfferOperationResponse { Success = false, Message = "Free plan limit reached. Please upgrade your plan." };
        }

        var lastOffer = await _context.Offers
            .Where(o => o.CompanyId == user.CompanyId)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync();

        var offerNumber = GenerateOfferNumber(lastOffer?.OfferNumber);

        var offer = new Offer
        {
            OfferNumber = offerNumber,
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            CustomerPhone = request.CustomerPhone,
            CustomerAddress = request.CustomerAddress,
            OfferDate = request.OfferDate,
            DueDate = request.DueDate,
            Currency = request.Currency,
            Notes = request.Notes,
            UserId = userId,
            CompanyId = user.CompanyId.Value,
            Items = request.Items.Select(item => new OfferItem
            {
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.Quantity * item.UnitPrice
            }).ToList()
        };

        offer.TotalAmount = offer.Items.Sum(i => i.TotalPrice);

        _context.Offers.Add(offer);

        company.OffersUsed += 1;

        await _context.SaveChangesAsync();

        var dto = await GetOfferByIdAsync(offer.Id, user.CompanyId.Value);

        return new OfferOperationResponse
        {
            Success = true,
            Offer = dto,
            Message = "Offer created"
        };
    }

    public async Task<OfferDto?> GetOfferByIdAsync(int id, int companyId)
    {
        var offer = await _context.Offers
            .Include(o => o.Items)
            .Include(o => o.Company)
            .FirstOrDefaultAsync(o => o.Id == id && o.CompanyId == companyId);

        if (offer == null) return null;

        return MapToDto(offer);
    }

    public async Task<List<OfferDto>> GetOffersByUserAsync(string userId, int page = 1, int pageSize = 10)
    {
        var offers = await _context.Offers
            .Include(o => o.Items)
            .Include(o => o.Company)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return offers.Select(MapToDto).ToList();
    }

    public async Task<List<OfferDto>> GetOffersByCompanyAsync(int companyId, int page = 1, int pageSize = 10)
    {
        var offers = await _context.Offers
            .Include(o => o.Items)
            .Include(o => o.Company)
            .Where(o => o.CompanyId == companyId)
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return offers.Select(MapToDto).ToList();
    }

    public async Task<OfferDto?> UpdateOfferAsync(int id, UpdateOfferRequest request)
    {
        var offer = await _context.Offers
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (offer == null) return null;


        offer.CustomerName = request.CustomerName;
        offer.CustomerEmail = request.CustomerEmail;
        offer.CustomerPhone = request.CustomerPhone;
        offer.CustomerAddress = request.CustomerAddress;
        offer.OfferDate = request.OfferDate;
        offer.DueDate = request.DueDate;
        offer.Currency = request.Currency;
        offer.Notes = request.Notes;

        // Update items
        _context.OfferItems.RemoveRange(offer.Items);
        offer.Items = request.Items.Select(item => new OfferItem
        {
            Description = item.Description,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            TotalPrice = item.Quantity * item.UnitPrice,
            OfferId = offer.Id
        }).ToList();

        offer.TotalAmount = offer.Items.Sum(i => i.TotalPrice);

        await _context.SaveChangesAsync();
        return await GetOfferByIdAsync(offer.Id, offer.CompanyId);
    }

    public async Task<bool> DeleteOfferAsync(int id, string userId)
    {
        var offer = await _context.Offers
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (offer == null) return false;

        _context.Offers.Remove(offer);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> SendOfferAsync(int id, string userId)
    {
        var offer = await _context.Offers
            .Include(o => o.Items)
            .Include(o => o.Company)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

        if (offer == null) return false;

        var emailSent = await _emailService.SendOfferEmailAsync(offer);
        if (emailSent)
        {
            offer.Status = OfferStatus.Sent;
            await _context.SaveChangesAsync();
        }

        return emailSent;
    }

    private string GenerateOfferNumber(string? lastOfferNumber)
    {
        if (string.IsNullOrEmpty(lastOfferNumber))
        {
            return $"TKF-{DateTime.Now:yyyyMM}-001";
        }

        var parts = lastOfferNumber.Split('-');
        if (parts.Length == 3 && int.TryParse(parts[2], out var lastNumber))
        {
            var nextNumber = lastNumber + 1;
            return $"TKF-{DateTime.Now:yyyyMM}-{nextNumber:D3}";
        }

        return $"TKF-{DateTime.Now:yyyyMM}-001";
    }

    private static OfferDto MapToDto(Offer offer)
    {
        return new OfferDto
        {
            Id = offer.Id,
            OfferNumber = offer.OfferNumber,
            CustomerName = offer.CustomerName,
            CustomerEmail = offer.CustomerEmail,
            CustomerPhone = offer.CustomerPhone,
            CustomerAddress = offer.CustomerAddress,
            OfferDate = offer.OfferDate,
            DueDate = offer.DueDate,
            Currency = offer.Currency,
            Notes = offer.Notes,
            TotalAmount = offer.TotalAmount,
            Status = offer.Status,
            CreatedAt = offer.CreatedAt,
            CompanyName = offer.Company.Name,
            Items = offer.Items.Select(item => new OfferItemDto
            {
                Id = item.Id,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                TotalPrice = item.TotalPrice
            }).ToList()
        };
    }
}