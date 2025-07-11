using OfferManagement.API.Models;
using System.Net;
using System.Net.Mail;
using System.Text;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
        var pdf = GenerateOfferPdf(offer);

        return await SendEmailAsync(offer.CustomerEmail, subject, body, pdf, $"{offer.OfferNumber}.pdf");
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, byte[]? attachmentData = null, string? attachmentName = null)
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

            if (attachmentData != null)
            {
                var stream = new MemoryStream(attachmentData);
                var attachment = new Attachment(stream, attachmentName ?? "attachment.pdf", "application/pdf");
                message.Attachments.Add(attachment);
            }

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

    private byte[] GenerateOfferPdf(Offer offer)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.DefaultTextStyle(TextStyle.Default.FontSize(12));

                page.Content().Column(column =>
                {
                    column.Item().Text(offer.Company.Name).FontSize(18).Bold();
                    column.Item().Text(offer.Company.Address);
                    column.Item().Text($"Telefon: {offer.Company.Phone}  E-posta: {offer.Company.Email}");
                    column.Item().PaddingVertical(10).LineHorizontal(1);

                    column.Item().Text($"Teklif No: {offer.OfferNumber}").Bold();
                    column.Item().Text($"Tarih: {offer.OfferDate:dd.MM.yyyy}");
                    column.Item().Text($"Müşteri: {offer.CustomerName}");
                    column.Item().Text($"Adres: {offer.CustomerAddress}");
                    column.Item().PaddingVertical(10).LineHorizontal(1);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.ConstantColumn(50);
                            columns.ConstantColumn(80);
                            columns.ConstantColumn(80);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Açıklama");
                            header.Cell().Element(HeaderCell).AlignCenter().Text("Adet");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Birim Fiyat");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Toplam");
                        });

                        foreach (var item in offer.Items)
                        {
                            table.Cell().Element(DataCell).Text(item.Description);
                            table.Cell().Element(DataCell).AlignCenter().Text(item.Quantity.ToString());
                            table.Cell().Element(DataCell).AlignRight().Text(item.UnitPrice.ToString("C"));
                            table.Cell().Element(DataCell).AlignRight().Text(item.TotalPrice.ToString("C"));
                        }

                        table.Footer(footer =>
                        {
                            footer.Cell().Element(DataCell).ColumnSpan(3).AlignRight().Text("Toplam");
                            footer.Cell().Element(DataCell).AlignRight().Text(offer.TotalAmount.ToString("C"));
                        });

                        static IContainer HeaderCell(IContainer container) =>
                            container.DefaultTextStyle(TextStyle.Default.SemiBold()).Padding(5).Background(Colors.Grey.Lighten2);

                        static IContainer DataCell(IContainer container) =>
                            container.Padding(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2);
                    });
                });
            });
        });

        return document.GeneratePdf();
    }
}