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
                page.DefaultTextStyle(TextStyle.Default.FontSize(11).FontFamily("Helvetica"));
                page.Background(Colors.White);

                page.Header().Background(Colors.Grey.Lighten3).Padding(10).Row(row =>
                {
                    row.ConstantColumn(120).Height(80).Image("wwwroot/"+offer.Company.Logo, ImageScaling.FitArea); // Logo

                    row.RelativeColumn().AlignRight().Column(col =>
                    {
                        col.Item().Text(offer.Company.Name).FontSize(18).Bold();
                        col.Item().Text(offer.Company.Address);
                        col.Item().Text($"Tel: {offer.Company.Phone}");
                        col.Item().Text($"E-posta: {offer.Company.Email}");
                    });
                });

                page.Content().PaddingVertical(10).Column(column =>
                {
                    column.Spacing(15);

                    column.Item().AlignCenter().Text($"Teklif: {offer.OfferNumber}").Bold().FontSize(20);

                    column.Item().Row(row =>
                    {
                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text("TEKLİF BİLGİLERİ").Bold().FontSize(12).Underline();
                            col.Item().Text($"Teklif No: {offer.OfferNumber}");
                            col.Item().Text($"Tarih: {offer.OfferDate:dd.MM.yyyy}");
                        });

                        row.RelativeColumn().Column(col =>
                        {
                            col.Item().Text("MÜŞTERİ BİLGİLERİ").Bold().FontSize(12).Underline();
                            col.Item().Text(offer.CustomerName);
                            col.Item().Text(offer.CustomerAddress);
                        });
                    });

                    column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(4);
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                            columns.RelativeColumn(2);
                        });

                        table.Header(header =>
                        {
                            header.Cell().Element(HeaderCell).Text("Ürün Açıklaması");
                            header.Cell().Element(HeaderCell).AlignCenter().Text("Adet");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Birim Fiyat");
                            header.Cell().Element(HeaderCell).AlignRight().Text("Tutar");
                        });

                        foreach (var item in offer.Items)
                        {
                            table.Cell().Element(DataCell).Text(item.Description);
                            table.Cell().Element(DataCell).AlignCenter().Text(item.Quantity.ToString());
                            table.Cell().Element(DataCell).AlignRight().Text(item.UnitPrice.ToString("C"));
                            table.Cell().Element(DataCell).AlignRight().Text(item.TotalPrice.ToString("C"));
                        }

                        // Ara toplam, KDV, genel toplam
                        table.Footer(footer =>
                        {
                            footer.Cell().ColumnSpan(3).Element(DataCell).AlignRight().Text("Ara Toplam").Bold();
                            footer.Cell().Element(DataCell).AlignRight().Text((offer.TotalAmount * 0.82m).ToString("C")).Bold();

                            footer.Cell().ColumnSpan(3).Element(DataCell).AlignRight().Text("KDV %18").Bold();
                            footer.Cell().Element(DataCell).AlignRight().Text((offer.TotalAmount * 0.18m).ToString("C")).Bold();

                            footer.Cell().ColumnSpan(3).Element(DataCell).AlignRight().Text("Genel Toplam").Bold().FontSize(11);
                            footer.Cell().Element(DataCell).AlignRight().Text(offer.TotalAmount.ToString("C")).Bold().FontSize(11);
                        });

                        static IContainer HeaderCell(IContainer container) =>
                            container.PaddingVertical(5).PaddingLeft(5).Background("#eeeeee").BorderBottom(1).BorderColor(Colors.Grey.Medium).DefaultTextStyle(TextStyle.Default.SemiBold().FontSize(11));

                        static IContainer DataCell(IContainer container) =>
                            container.PaddingVertical(4).PaddingLeft(5).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).DefaultTextStyle(TextStyle.Default.FontSize(10));
                    });

                    column.Item().PaddingTop(20).Row(row =>
                    {
                        row.RelativeColumn().Text("Not: Bu teklif belge niteliğindedir. Siparişe dönüşmeden fatura oluşturulmaz.")
                            .Italic().FontSize(9);

                        row.ConstantColumn(200).Column(col =>
                        {
                            col.Item().Text("Yetkili İmza").AlignRight().FontSize(10).Bold();
                            col.Item().Height(40);
                            col.Item().Container().AlignRight().Width(150).LineHorizontal(1);
                        });
                    });
                });

                page.Footer().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                page.Footer().AlignRight().Text(text =>
                {
                    text.Span("Sayfa ").FontSize(9);
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        return document.GeneratePdf();
    }

}
