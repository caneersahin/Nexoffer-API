using OfferManagement.API.Models;

namespace OfferManagement.API.Services;

public interface IEmailService
{
    Task<bool> SendOfferEmailAsync(Offer offer);
    Task<bool> SendEmailAsync(string to, string subject, string body, byte[]? attachmentData = null, string? attachmentName = null);
}