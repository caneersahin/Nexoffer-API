using OfferManagement.API.Models;

namespace OfferManagement.API.DTOs;

public class UpgradePlanRequest
{
    public SubscriptionPlan Plan { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
}
