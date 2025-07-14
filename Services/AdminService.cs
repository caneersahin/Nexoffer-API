using Microsoft.EntityFrameworkCore;
using OfferManagement.API.Data;
using OfferManagement.API.DTOs;
using OfferManagement.API.Models;

namespace OfferManagement.API.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;
    private readonly ICompanyService _companyService;

    public AdminService(ApplicationDbContext context, ICompanyService companyService)
    {
        _context = context;
        _companyService = companyService;
    }

    public async Task<AdminDashboardDto> GetDashboardAsync()
    {
        var companies = await _context.Companies
            .Include(c => c.Users)
            .Include(c => c.Offers)
            .ToListAsync();

        var companySummaries = companies.Select(c => new CompanySummaryDto
        {
            Id = c.Id,
            Name = c.Name,
            OfferCount = c.Offers.Count,
            UserCount = c.Users.Count,
            SubscriptionPlan = c.SubscriptionPlan
        }).ToList();

        var totalCompanies = companies.Count;
        var proCompanies = companies.Count(c => c.SubscriptionPlan == SubscriptionPlan.Pro);
        var freeCompanies = companies.Count(c => c.SubscriptionPlan == SubscriptionPlan.Free);

        var today = DateTime.UtcNow.Date;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek);
        var startOfMonth = new DateTime(today.Year, today.Month, 1);

        var payments = await _context.Payments.ToListAsync();
        var revenueToday = payments.Where(p => p.PaidAt.Date == today).Sum(p => p.Amount);
        var revenueWeek = payments.Where(p => p.PaidAt.Date >= startOfWeek).Sum(p => p.Amount);
        var revenueMonth = payments.Where(p => p.PaidAt.Date >= startOfMonth).Sum(p => p.Amount);
        var revenueTotal = payments.Sum(p => p.Amount);

        return new AdminDashboardDto
        {
            Companies = companySummaries,
            TotalCompanies = totalCompanies,
            ProCompanies = proCompanies,
            FreeCompanies = freeCompanies,
            RevenueToday = revenueToday,
            RevenueThisWeek = revenueWeek,
            RevenueThisMonth = revenueMonth,
            RevenueTotal = revenueTotal
        };
    }

    public async Task<CompanyDto?> UpgradeCompanyAsync(int companyId, UpgradePlanRequest request)
    {
        return await _companyService.UpgradePlanAsync(companyId, request);
    }
}
