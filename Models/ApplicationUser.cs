using Microsoft.AspNetCore.Identity;

namespace OfferManagement.API.Models;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int? CompanyId { get; set; }
    public Company? Company { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public ICollection<Offer> Offers { get; set; } = new List<Offer>();
}