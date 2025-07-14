namespace OfferManagement.API.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public DateTime PaidAt { get; set; }
    public string? TransactionId { get; set; }
}

public class RecordPaymentRequest
{
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
}
