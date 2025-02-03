/* InvoiceService.cs
* Invoicing App
* Liam Conn
* Implements IInvoiceService, provides methods to manage
* invoice data for retrieval, creation, updating,
* and operating on invoice line items
*
*/

using Invoicing.Entities;
using Invoicing.Interfaces;
using InvoicingApp.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;

namespace InvoicingApp.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly InvoicingDbContext _context;

        public InvoiceService(InvoicingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieve all invoices associated with the specified customer
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<Invoice>> GetInvoicesByCustomerIdAsync(int customerId)
        {
            return await _context.Invoices
                .Where(i => i.CustomerId == customerId)
                .Include(i => i.LineItems)
                .Include(i => i.PaymentTerms)
                .ToListAsync();
        }

        /// <summary>
        /// Get specific invoice by its primary key
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            // includes line items and payments terms attached to invoice
            return await _context.Invoices
                .Include(i => i.LineItems)
                .Include(i => i.PaymentTerms)
                .FirstOrDefaultAsync(i => i.InvoiceId == id);
        }

        /// <summary>
        /// Adds new invoice to dataset.
        /// </summary>
        /// <param name="invoice">invoice object</param>
        /// <returns></returns>
        public async Task AddInvoiceAsync(Invoice invoice)
        {
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Add new invoice line item to dataset attached to invoice
        /// </summary>
        /// <param name="invoiceId">primary key of invoice</param>
        /// <param name="lineItem">invoice line item object</param>
        /// <returns></returns>
        public async Task AddInvoiceLineItemAsync(int invoiceId, InvoiceLineItem lineItem)
        {
            // Ensure the line item is associated with the correct invoice
            lineItem.InvoiceId = invoiceId;

            // Add the line item to the database
            _context.InvoiceLineItems.Add(lineItem);

            // Save changes
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Get customer id that's attached to specific invoice
        /// </summary>
        /// <param name="invoiceId">primary key of invoice</param>
        /// <returns></returns>
        public async Task<int?> GetCustomerIdByInvoiceIdAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Where(i => i.InvoiceId == invoiceId)
                .Select(i => i.CustomerId)
                .FirstOrDefaultAsync();

            return invoice;
        }

        /// <summary>
        ///  Get all available payment terms for invoices
        /// </summary>
        /// <returns>IEnumerable of payment terms</returns>
        public async Task<IEnumerable<PaymentTerms>> GetPaymentTermsAsync()
        {
            return await _context.PaymentTerms.ToListAsync();
        }
    }
}
