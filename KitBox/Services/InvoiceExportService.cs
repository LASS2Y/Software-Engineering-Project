using System;
using System.Globalization;
using System.IO;
using System.Text;
using KitBox.Services.Interfaces;

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
        sb.AppendLine($"Bill id: {(request.BillId.HasValue ? request.BillId.Value.ToString(CultureInfo.InvariantCulture) : "N/A")}");
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
}