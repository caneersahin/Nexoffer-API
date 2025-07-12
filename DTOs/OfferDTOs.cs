using OfferManagement.API.Models;

namespace OfferManagement.API.DTOs;

public class OfferDto
{
    public int Id { get; set; }
    public string OfferNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string CustomerAddress { get; set; } = string.Empty;
    public DateTime OfferDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Currency Currency { get; set; }
    public string? Notes { get; set; }
    public decimal TotalAmount { get; set; }
    public OfferStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public List<OfferItemDto> Items { get; set; } = new();
}

public class OfferItemDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

public class CreateOfferRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string CustomerAddress { get; set; } = string.Empty;
    public DateTime OfferDate { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public Currency Currency { get; set; } = Currency.TRY;
    public string? Notes { get; set; }
    public List<CreateOfferItemRequest> Items { get; set; } = new();
}

public class CreateOfferItemRequest
{
    public string Description { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public decimal UnitPrice { get; set; }
}

public class UpdateOfferRequest
{
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }
    public string CustomerAddress { get; set; } = string.Empty;
    public DateTime OfferDate { get; set; }
    public DateTime? DueDate { get; set; }
    public Currency Currency { get; set; }
    public string? Notes { get; set; }
    public List<CreateOfferItemRequest> Items { get; set; } = new();
}

public class OfferOperationResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public OfferDto? Offer { get; set; }
}