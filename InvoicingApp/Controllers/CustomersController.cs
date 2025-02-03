/* CustomersController.cs
* Invoicing App
* Liam Conn
* Customer Controller endpoint for customer-related operations.
* Endpoints for retrieving, creating, updating, deleting, restoring
* customer data. Integrates with ICustomerService Interface.
*
*/

using Invoicing.Entities;
using Invoicing.Interfaces;
using InvoicingApp.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace InvoicingApp.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomerService _customerService;

        public CustomersController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Display list of customers, filter for specified alpha group.
        /// </summary>
        /// <param name="group">alpha group (ex. "A-E") to filter customers. Default "A-E"</param>
        /// <returns>list of customers for specified group</returns>
        [HttpGet("/customers")]
        public async Task<IActionResult> AllCustomers(string group = "A-E")
        {
            await _customerService.CleanupPendingDeletes();
            var customers = await _customerService.GetCustomersByGroupAsync(group);
            ViewBag.Group = group;
            return View(customers); 
        }

        /// <summary>
        /// Display form for creating new customer.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public IActionResult Add(string group = "A-E")
        {
            ViewBag.Group = group;
            return View(new Customer());
        }

        /// <summary>
        /// Handle submit of new customer form, add customer to system
        /// </summary>
        /// <param name="customer">customer object containing submitted data from form</param>
        /// <param name="group">pass group to redirect back to correct group page.</param>
        /// <returns>redirect to customer group page if successful or display validation errors</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Customer customer, string group = "A-E")
        {
            // passes customer validation
            if (ModelState.IsValid)
            {
                await _customerService.AddCustomerAsync(customer);
                return RedirectToAction(nameof(AllCustomers), new {group});
            }
            // generic error for validation errors
            ModelState.AddModelError(string.Empty, "There are errors in the form. Please correct them and try again.");
            ViewBag.Group = group;
            return View(customer);
        }

        /// <summary>
        /// Display form for editing existing customer
        /// </summary>
        /// <param name="id">primary key of existing customer</param>
        /// <param name="group">pass group to redirect back to correct group page.</param>
        /// <returns>existing customer view to edit fields</returns>
        [HttpGet("/Customers/Edit/{group}")]
        public async Task<IActionResult> Edit(int id, string group = "A-E")
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) // customer does not exist
                return NotFound();

            ViewBag.Group = group;
            return View(customer);
        }

        /// <summary>
        /// Handle form submission of customer edits and update customer record.
        /// </summary>
        /// <param name="customer">primary key of customer</param>
        /// <param name="group">pass group to redirect back to correct group page.</param>
        /// <returns>redirect back to AllCustomers view if successful, or display validation errors for customer</returns>
        [HttpPost("/Customers/Edit/{group}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer, string group = "A-E")
        {
            if (ModelState.IsValid)
            {
                await _customerService.UpdateCustomerAsync(customer);
                return RedirectToAction(nameof(AllCustomers), new {group});
            }
            // generic error for validation errors
            ModelState.AddModelError(string.Empty, "There are errors in the form. Please correct them and try again.");
            ViewBag.Group = group;
            return View(customer);
        }

        /// <summary>
        /// Marks customer as deleted (soft delete)
        /// </summary>
        /// <param name="id">primary key of customer to be deleted</param>
        /// <param name="group">alpha customer group to return the user to</param>
        /// <returns>redirect to AllCustomers view after deletion</returns>
        [HttpGet("/Customers/Delete/{id}/{group}")]
        public async Task<IActionResult> Delete(int id, string group = "A-E")
        {
            // Retrieve the customer to get their name before soft delete
            var customer = await _customerService.GetCustomerByIdAsync(id);

            if (customer == null) // invalid customer
            {
                TempData["ErrorMessage"] = "Customer not found.";
                return RedirectToAction(nameof(AllCustomers), new { group });
            }

            // Perform the soft delete
            await _customerService.SoftDeleteCustomerAsync(id);

            // Redirect to AllCustomers and pass the undo ID/customer name in TempData
            TempData["UndoCustomerId"] = id;
            TempData["UndoCustomerName"] = customer.Name;

            ViewBag.Group = group;
            return RedirectToAction(nameof(AllCustomers), new { group });
        }

        
        [HttpPost("/Customers/HardDelete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HardDelete(int id, string group = "A-E")
        {
            await _customerService.HardDeleteCustomerAsync(id);

            ViewBag.Group = group;
            return RedirectToAction(nameof(AllCustomers), new { group });

        }
        

        /// <summary>
        /// Clear the customer id from temp data for undo
        /// </summary>
        /// <returns></returns>
        [HttpPost("/Customers/ClearUndoCustomerId")]
        public IActionResult ClearUndoCustomerId()
        {
            TempData.Remove("UndoCustomerId");
            return Ok();
        }

        /// <summary>
        /// Restore soft-deleted customer
        /// </summary>
        /// <param name="id">primary key of customer record</param>
        /// <param name="group">alpha group to return to correct customer group</param>
        /// <returns>redirect to AllCustomers view after restoring</returns>
        [HttpGet("/Customers/UndoDelete/{id}/{group?}")]
        public async Task<IActionResult> UndoDelete(int id, string group = "A-E")
        {
            await _customerService.UndoDeleteCustomerAsync(id);

            TempData.Remove("UndoCustomerId");
            ViewBag.Group = group;
            return RedirectToAction(nameof(AllCustomers), new { group });
        }
    }
}