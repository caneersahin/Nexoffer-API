namespace OfferManagement.API.DTOs;

public class CompanyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Logo { get; set; }
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? IBAN { get; set; }
    public string? Website { get; set; }
    public string SubscriptionPlan { get; set; } = string.Empty;
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
    public int OffersUsed { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateCompanyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? TaxNumber { get; set; }
    public string? IBAN { get; set; }
    public string? Website { get; set; }
}