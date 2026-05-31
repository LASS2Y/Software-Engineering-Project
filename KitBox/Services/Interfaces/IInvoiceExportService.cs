using System;

namespace KitBox.Services.Interfaces;

public record InvoiceExportRequest(
    string DocumentType,
    int OrderId,
    int? BillId,
    string CustomerName,
    string CustomerEmail,
    DateTime IssuedAt,
    decimal TotalAmount,
    decimal DepositAmount,
    decimal AmountPaid,
    decimal RemainingAmount,
    string Notes,
    DateTime? EstimatedAvailableDate = null
);

/// <summary>
/// Exports order payment documents as txt files.
/// </summary>
public interface IInvoiceExportService
{
    /// <summary>
    /// Creates a txt document in the user's Downloads folder and returns the file path.
    /// </summary>
    string ExportTxt(InvoiceExportRequest request);
    
    string ExportPdf(InvoiceExportRequest request);
}