using System;
using System.Globalization;
using System.IO;
using System.Text;
using KitBox.Services.Interfaces;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KitBox.Services;

public class InvoiceExportService : IInvoiceExportService
{
    public string ExportTxt(InvoiceExportRequest request)
    {
        string downloads = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        Directory.CreateDirectory(downloads);

        string safeType = request.DocumentType.Replace(' ', '_');
        string fileName = $"kitbox_{safeType}_order_{request.OrderId}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
        string fullPath = Path.Combine(downloads, fileName);

        string content = BuildContent(request);
        File.WriteAllText(fullPath, content, Encoding.UTF8);

        return fullPath;
    }

    private static string BuildContent(InvoiceExportRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine("KITBOX - PAYMENT DOCUMENT");
        sb.AppendLine("========================================");
        sb.AppendLine($"Document type: {request.DocumentType}");
        sb.AppendLine($"Issue date: {request.IssuedAt:dd/MM/yyyy}");
        sb.AppendLine($"Order id: {request.OrderId}");
        sb.AppendLine(
            $"Bill id: {(request.BillId.HasValue ? request.BillId.Value.ToString(CultureInfo.InvariantCulture) : "N/A")}");
        sb.AppendLine();
        sb.AppendLine("Customer");
        sb.AppendLine($"Name: {request.CustomerName}");
        sb.AppendLine($"Email: {request.CustomerEmail}");
        sb.AppendLine();
        sb.AppendLine("Amounts");
        sb.AppendLine($"Total amount: EUR {request.TotalAmount:F2}");
        sb.AppendLine($"Deposit amount: EUR {request.DepositAmount:F2}");
        sb.AppendLine($"Paid amount: EUR {request.AmountPaid:F2}");
        sb.AppendLine($"Remaining amount: EUR {request.RemainingAmount:F2}");
        if (request.EstimatedAvailableDate.HasValue)
            sb.AppendLine($"Estimated availability date: {request.EstimatedAvailableDate.Value:dd/MM/yyyy}");
        sb.AppendLine();
        sb.AppendLine("Notes");
        sb.AppendLine(request.Notes);
        sb.AppendLine("========================================");

        return sb.ToString();
    }

    

    public string ExportPdf(InvoiceExportRequest request)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        string downloads = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
            "Downloads");

        Directory.CreateDirectory(downloads);

        string safeType = string.IsNullOrWhiteSpace(request.DocumentType)
            ? "document"
            : string.Join("_",
                request.DocumentType.Trim()
                    .Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));

        string fileName = $"kitbox_{safeType}_order_{request.OrderId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        string fullPath = Path.Combine(downloads, fileName);

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(32);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text("KITBOX - PAYMENT DOCUMENT")
                        .Bold()
                        .FontSize(18)
                        .FontColor(Colors.Blue.Medium);

                    header.Item()
                        .PaddingTop(6)
                        .LineHorizontal(1)
                        .LineColor(Colors.Grey.Lighten1);
                });

                page.Content().PaddingTop(16).Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Text("Document information")
                        .Bold()
                        .FontSize(13);

                    column.Item().Text($"Document type: {request.DocumentType ?? "-"}");
                    column.Item().Text($"Issue date: {request.IssuedAt:dd/MM/yyyy}");
                    column.Item().Text($"Order id: {request.OrderId}");
                    column.Item().Text($"Bill id: {(request.BillId.HasValue ? request.BillId.Value.ToString(CultureInfo.InvariantCulture) : "N/A")}");

                    column.Item().PaddingTop(8).Text("Customer")
                        .Bold()
                        .FontSize(13);

                    column.Item().Text($"Name: {request.CustomerName ?? "-"}");
                    column.Item().Text($"Email: {request.CustomerEmail ?? "-"}");

                    column.Item().PaddingTop(8).Text("Amounts")
                        .Bold()
                        .FontSize(13);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Cell().Text("Total amount");
                        table.Cell().AlignRight()
                            .Text($"EUR {request.TotalAmount.ToString("F2", CultureInfo.InvariantCulture)}");

                        table.Cell().Text("Deposit amount");
                        table.Cell().AlignRight()
                            .Text($"EUR {request.DepositAmount.ToString("F2", CultureInfo.InvariantCulture)}");

                        table.Cell().Text("Paid amount");
                        table.Cell().AlignRight()
                            .Text($"EUR {request.AmountPaid.ToString("F2", CultureInfo.InvariantCulture)}");

                        table.Cell().Text("Remaining amount");
                        table.Cell().AlignRight()
                            .Text($"EUR {request.RemainingAmount.ToString("F2", CultureInfo.InvariantCulture)}");
                    });

                    if (request.EstimatedAvailableDate.HasValue)
                    {
                        column.Item().PaddingTop(8)
                            .Text($"Estimated availability date: {request.EstimatedAvailableDate.Value:dd/MM/yyyy}");
                    }

                    column.Item().PaddingTop(8).Text("Notes")
                        .Bold()
                        .FontSize(13);

                    column.Item()
                        .Border(1)
                        .BorderColor(Colors.Grey.Lighten2)
                        .Background(Colors.Grey.Lighten5)
                        .Padding(10)
                        .Text(string.IsNullOrWhiteSpace(request.Notes) ? "-" : request.Notes);
                });

                page.Footer().PaddingTop(10).AlignCenter().Text(text =>
                {
                    text.Span("Generated on ");
                    text.Span(DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm"));
                    text.Span(" | Page ");
                    text.CurrentPageNumber();
                    text.Span(" / ");
                    text.TotalPages();
                });
            });
        });

        try
        {
            document.GeneratePdf(fullPath);
            return fullPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to generate PDF.", ex);
            
        }
    }
}
