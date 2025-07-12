using System.ComponentModel.DataAnnotations;

namespace OfferManagement.API.Models;

public class Product
{
    public int Id { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Category { get; set; }

    public decimal Price { get; set; }

    public int CompanyId { get; set; }

    public Company Company { get; set; } = null!;
}
