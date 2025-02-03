/* InvoicesController.cs
* Invoicing App
* Liam Conn
* Controller for managing invoices, retrieval, creation,
* operations on invoice line items.
*
*/

using Invoicing.Entities;
using Invoicing.Interfaces;
using InvoicingApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InvoicingApp.Controllers
{
    public class InvoicesController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly ICustomerService _customerService;

        public InvoicesController(IInvoiceService invoiceService, ICustomerService customerService)
        {
            _invoiceService = invoiceService;
            _customerService = customerService;
        }

        /// <summary>
        /// Display list of invoices for specifc customer
        /// </summary>
        /// <param name="customerId">customer primary key</param>
        /// <param name="group">pass group parameter to return to correct customer group</param>
        /// <param name="selectedInvoiceId">invoice primary key</param>
        /// <returns>view displaying list of invoices for customer</returns>
        [HttpGet("/Invoices/AllInvoices/{customerId}/{group?}")]
        public async Task<IActionResult> AllInvoices(int customerId, string group = "A-E", int? selectedInvoiceId = null)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer == null) // invalid customer
            {
                return NotFound();
            }

            var invoices = await _invoiceService.GetInvoicesByCustomerIdAsync(customerId);
            var paymentTerms = await _invoiceService.GetPaymentTermsAsync(); // Fetch PaymentTerms

            // fields to pass to AllInvoices to display customer/invoice info
            ViewBag.CustomerName = customer.Name;
            ViewBag.CustomerAddress = customer.Address1;
            ViewBag.CustomerCity = customer.City;
            ViewBag.CustomerId = customerId;
            ViewBag.Group = group; // Pass the group to the view
            ViewBag.PaymentTerms = paymentTerms; // Pass PaymentTerms for dropdown

            // Get the selected invoice
            var selectedInvoice = selectedInvoiceId.HasValue
                ? invoices.FirstOrDefault(i => i.InvoiceId == selectedInvoiceId.Value)
                : invoices.FirstOrDefault();

            ViewBag.SelectedInvoiceId = selectedInvoice?.InvoiceId;

            // Set CustomerTerms based on the selected invoice
            if (selectedInvoice != null)
            {
                var matchedTerms = paymentTerms.FirstOrDefault(pt => pt.PaymentTermsId == selectedInvoice.PaymentTermsId);
                ViewBag.CustomerTerms = matchedTerms?.Description ?? "N/A";
                ViewBag.CurrentPaymentTermsId = selectedInvoice.PaymentTermsId; // Set CurrentPaymentTermsId
            }
            else
            {
                ViewBag.CustomerTerms = "N/A"; // Default when no invoice is selected
                ViewBag.CurrentPaymentTermsId = null; // Default when no invoice is selected
            }

            return View(invoices);

        }

        /// <summary>
        /// Displays details of specific invoice
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceService.GetInvoiceByIdAsync(id);
            if (invoice == null)
                return NotFound();

            return View(invoice);
        }

        /// <summary>
        /// Handles submission of invoice created from form, add to invoice dataset
        /// </summary>
        /// <param name="customerId">customer primary key</param>
        /// <param name="invoice">invoice object</param>
        /// <param name="group">pass group to redirect back to correct customer group</param>
        /// <returns>redirect to AllInvoices view on success</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddInvoice(int customerId, Invoice invoice, string group)
        {
            // Manually set the CustomerId
            invoice.CustomerId = customerId;

            // Ignore navigation properties
            ModelState.Remove(nameof(invoice.Customer));
            ModelState.Remove(nameof(invoice.PaymentTerms));

            if (!ModelState.IsValid) // invalid invoice to add
            {
                return RedirectToAction(nameof(AllInvoices), new { customerId, group });
            }

            await _invoiceService.AddInvoiceAsync(invoice);

            return RedirectToAction(nameof(AllInvoices), new { customerId, group });
        }

        /// <summary>
        /// Handle submitting line item form and add invoice line item to dataset
        /// </summary>
        /// <param name="invoiceId">primary key of invoice</param>
        /// <param name="lineItem">invoice line item object</param>
        /// <param name="group"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddLineItem(int invoiceId, InvoiceLineItem lineItem, string group)
        {
            if (ModelState.IsValid)
            {
                await _invoiceService.AddInvoiceLineItemAsync(invoiceId, lineItem);

                // Fetch the customer ID for redirection
                var customerId = await _invoiceService.GetCustomerIdByInvoiceIdAsync(invoiceId);

                // Redirect explicitly to the AllInvoices action with all required route parameters
                return RedirectToAction("AllInvoices", "Invoices", new { customerId, selectedInvoiceId = invoiceId, group });
            }

            // In case of validation errors, redirect back to the same invoice
            var fallbackCustomerId = await _invoiceService.GetCustomerIdByInvoiceIdAsync(invoiceId);
            return RedirectToAction("AllInvoices", "Invoices", new { customerId = fallbackCustomerId, selectedInvoiceId = invoiceId, group });
        }
    }
}
