using OnlineSalesManagementSystem.Domain.Entities;

namespace OnlineSalesManagementSystem.Services.Sales;

public sealed class InvoiceTotalsService : IInvoiceTotalsService
{
    public void Recalculate(Invoice invoice)
    {
        if (invoice == null) throw new ArgumentNullException(nameof(invoice));

        invoice.Items ??= new List<InvoiceItem>();

        decimal subTotal = 0m;

        foreach (var item in invoice.Items)
        {
            // An toàn dữ liệu
            if (item.Qty < 0) item.Qty = 0;
            if (item.UnitPrice < 0) item.UnitPrice = 0;

            item.LineTotal = item.UnitPrice * item.Qty;
            subTotal += item.LineTotal;
        }

        invoice.SubTotal = subTotal;
        invoice.GrandTotal = subTotal;

        if (invoice.PaidAmount < 0) invoice.PaidAmount = 0;

        invoice.Status = invoice.PaidAmount >= invoice.GrandTotal
            ? InvoiceStatus.Paid
            : InvoiceStatus.Unpaid;
    }
}
