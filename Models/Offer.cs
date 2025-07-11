using System.ComponentModel.DataAnnotations;

namespace OfferManagement.API.Models;

public class Offer
{
    public int Id { get; set; }
    
    [Required]
    public string OfferNumber { get; set; } = string.Empty;
    
    [Required]
    public string CustomerName { get; set; } = string.Empty;
    
    [Required]
    public string CustomerEmail { get; set; } = string.Empty;
    
    public string? CustomerPhone { get; set; }
    
    [Required]
    public string CustomerAddress { get; set; } = string.Empty;
    
    public DateTime OfferDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? DueDate { get; set; }
    
    public Currency Currency { get; set; } = Currency.TRY;
    
    public string? Notes { get; set; }
    
    public decimal TotalAmount { get; set; }
    
    public OfferStatus Status { get; set; } = OfferStatus.Draft;
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public string UserId { get; set; } = string.Empty;
    
    public ApplicationUser User { get; set; } = null!;
    
    public int CompanyId { get; set; }
    
    public Company Company { get; set; } = null!;
    
    public ICollection<OfferItem> Items { get; set; } = new List<OfferItem>();
}

public class OfferItem
{
    public int Id { get; set; }
    
    [Required]
    public string Description { get; set; } = string.Empty;
    
    public int Quantity { get; set; } = 1;
    
    public decimal UnitPrice { get; set; }
    
    public decimal TotalPrice { get; set; }
    
    public int OfferId { get; set; }
    
    public Offer Offer { get; set; } = null!;
}

public enum Currency
{
    TRY,
    USD,
    EUR
}

public enum OfferStatus
{
    Draft,
    Sent,
    Accepted,
    Rejected,
    Expired
}