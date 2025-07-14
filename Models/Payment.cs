using System.ComponentModel.DataAnnotations;

namespace OfferManagement.API.Models;

public class Payment
{
    public int Id { get; set; }

    [Required]
    public int CompanyId { get; set; }
    public Company Company { get; set; } = null!;

    [Required]
    public decimal Amount { get; set; }

    public DateTime PaidAt { get; set; } = DateTime.UtcNow;

    public string? TransactionId { get; set; }
}
