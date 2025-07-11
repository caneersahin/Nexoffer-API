using OfferManagement.API.Models;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace OfferManagement.API.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<bool> SendOfferEmailAsync(Offer offer)
    {
        var subject = $"Teklif - {offer.OfferNumber}";
        var body = GenerateOfferEmailBody(offer);

        return await SendEmailAsync(offer.CustomerEmail, subject, body);
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var smtpServer = _configuration["Email:SmtpServer"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"]!);
            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];

            using var client = new SmtpClient(smtpServer, smtpPort)
            {
                Credentials = new NetworkCredential(username, password),
                EnableSsl = true
            };

            var message = new MailMessage
            {
                From = new MailAddress(username!, "Teklif Sistemi"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);

            await client.SendMailAsync(message);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private string GenerateOfferEmailBody(Offer offer)
    {
        var sb = new StringBuilder();
        sb.AppendLine("<html><body>");
        sb.AppendLine($"<h2>Teklif: {offer.OfferNumber}</h2>");
        sb.AppendLine($"<p>Sayın {offer.CustomerName},</p>");
        sb.AppendLine($"<p>Talebiniz doğrultusunda hazırladığımız teklif aşağıdadır:</p>");
        sb.AppendLine("<table border='1' style='border-collapse: collapse; width: 100%;'>");
        sb.AppendLine("<tr><th>Açıklama</th><th>Adet</th><th>Birim Fiyat</th><th>Toplam</th></tr>");

        foreach (var item in offer.Items)
        {
            sb.AppendLine($"<tr><td>{item.Description}</td><td>{item.Quantity}</td><td>{item.UnitPrice:C}</td><td>{item.TotalPrice:C}</td></tr>");
        }

        sb.AppendLine("</table>");
        sb.AppendLine($"<p><strong>Toplam Tutar: {offer.TotalAmount:C}</strong></p>");
        sb.AppendLine($"<p>Teklif Geçerlilik Tarihi: {offer.DueDate:dd/MM/yyyy}</p>");
        sb.AppendLine("<p>Teşekkür ederiz.</p>");
        sb.AppendLine("</body></html>");

        return sb.ToString();
    }
}