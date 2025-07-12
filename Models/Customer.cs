using System.ComponentModel.DataAnnotations;

namespace OfferManagement.API.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;
}
