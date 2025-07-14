namespace OfferManagement.API.DTOs;

using OfferManagement.API.Models;

public class CompanySummaryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int OfferCount { get; set; }
    public int UserCount { get; set; }
    public SubscriptionPlan SubscriptionPlan { get; set; }
}

public class AdminDashboardDto
{
    public List<CompanySummaryDto> Companies { get; set; } = new();
    public int TotalCompanies { get; set; }
    public int ProCompanies { get; set; }
    public int FreeCompanies { get; set; }
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisWeek { get; set; }
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueTotal { get; set; }
}
