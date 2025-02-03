/* IInvoiceService.cs
* Invoicing App
* Liam Conn
* Interface methods for managing invoices,
* and its related data sets, creation, retrieval,
* line item operations.
*
*/

using Invoicing.Entities;

namespace Invoicing.Interfaces
{
    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId);
        Task<Invoice?> GetInvoiceByIdAsync(int id);
        Task<IEnumerable<PaymentTerms>> GetPaymentTermsAsync();
        Task AddInvoiceAsync(Invoice invoice);
        Task<int?> GetCustomerIdByInvoiceIdAsync(int invoiceId);
        Task AddInvoiceLineItemAsync(int invoiceId, InvoiceLineItem lineItem);
    }
}
