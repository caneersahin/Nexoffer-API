using System.ComponentModel.DataAnnotations;

namespace OfferManagement.API.Models;

public class Company
{
    public int Id { get; set; }
    
    [Required]
    public string Name { get; set; } = string.Empty;
    
    public string? Logo { get; set; }
    
    [Required]
    public string Address { get; set; } = string.Empty;
    
    [Required]
    public string Phone { get; set; } = string.Empty;
    
    [Required]
    public string Email { get; set; } = string.Empty;
    
    public string? TaxNumber { get; set; }
    
    public string? IBAN { get; set; }
    
    public string? Website { get; set; }
    
    public SubscriptionPlan SubscriptionPlan { get; set; } = SubscriptionPlan.Free;

    /// <summary>
    /// Tracks how many offers have been created while on the free plan.
    /// </summary>
    public int OffersUsed { get; set; } = 0;
    
    public DateTime SubscriptionStartDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? SubscriptionEndDate { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();

    public ICollection<Offer> Offers { get; set; } = new List<Offer>();

    public ICollection<Product> Products { get; set; } = new List<Product>();

    public ICollection<Customer> Customers { get; set; } = new List<Customer>();

    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum SubscriptionPlan
{
    Free,
    Pro,
    Enterprise
}