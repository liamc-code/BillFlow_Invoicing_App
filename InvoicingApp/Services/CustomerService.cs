/* CustomerService.cs
* Invoicing App
* Liam Conn
* Implements ICustomerService, provides methods to manage
* customer data for retrieval, creation, updating,
* soft deleting/deleting, and undoing of deleting actions
*
*/

using Invoicing.Entities;
using Invoicing.Interfaces;
using InvoicingApp.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;

namespace InvoicingApp.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly InvoicingDbContext _context;

        public CustomerService(InvoicingDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieve list of customers for the group specified
        /// </summary>
        /// <param name="group">Alpha group (ex. "A-E", "F-K", etc) to filter customers</param>
        /// <returns>IEnumerable of customers in specified group</returns>
        /// <exception cref="ArgumentException">catch out of bounds groups</exception>
        public async Task<IEnumerable<Customer>> GetCustomersByGroupAsync(string group)
        {
            var range = group.Split('-');
            if (range.Length != 2 || range[0].Length != 1 || range[1].Length != 1)
                throw new ArgumentException($"Invalid group range: {group}");

            var start = range[0].ToUpper();
            var end = range[1].ToUpper();

            if (string.Compare(start, end) > 0)
                throw new ArgumentException($"Invalid range: start '{start}' cannot be greater than end '{end}'");

            // filter out customers within group range
            var customers = await _context.Customers
                .Where(c => !c.IsDeleted &&
                            c.Name != null &&
                            string.Compare(c.Name.Substring(0, 1), start) >= 0 &&
                            string.Compare(c.Name.Substring(0, 1), end) <= 0)
                .OrderBy(c => c.Name) // Explicitly sort by Name
                .ToListAsync();

            return customers;
        }

        /// <summary>
        /// Get customer by unique id
        /// </summary>
        /// <param name="id">primary key of customer</param>
        /// <returns>customer entity (not marked for deletion)</returns>
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Invoices)
                .IgnoreQueryFilters() // Ensure we can fetch soft-deleted records
                .FirstOrDefaultAsync(c => c.CustomerId == id && !c.IsDeleted);
        }

        /// <summary>
        /// Add new customer to dataset
        /// </summary>
        /// <param name="customer">customer object to add</param>
        /// <returns></returns>
        public async Task AddCustomerAsync(Customer customer)
        {
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Updates (from edits) details of existing customer
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task UpdateCustomerAsync(Customer customer)
        {
            var existingCustomer = await _context.Customers.FindAsync(customer.CustomerId);

            // Update fields
            existingCustomer.Name = customer.Name;
            existingCustomer.Address1 = customer.Address1;
            existingCustomer.Address2 = customer.Address2;
            existingCustomer.City = customer.City;
            existingCustomer.ProvinceOrState = customer.ProvinceOrState;
            existingCustomer.ZipOrPostalCode = customer.ZipOrPostalCode;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.ContactFirstName = customer.ContactFirstName;
            existingCustomer.ContactLastName = customer.ContactLastName;
            existingCustomer.ContactEmail = customer.ContactEmail;

            // Save changes
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Mark customer for deletion in customer table without removing the record yet
        /// </summary>
        /// <param name="id">primary key of customer record</param>
        /// <returns></returns>
        public async Task SoftDeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // set field to true for delete
                customer.IsDeleted = true;
                // mark the datetime of the operation
                customer.DeletedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        
        public async Task HardDeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if(customer != null && customer.IsDeleted)
            {
                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }
        }
        

        /// <summary>
        /// Cleanup customers marked for deletion after time expiry
        /// </summary>
        /// <returns></returns>
        public async Task CleanupPendingDeletes()
        {
            var cutoffTime = DateTime.UtcNow.AddSeconds(-10); // 10 second grace period
            var pendingDeletes = await _context.Customers
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted && c.DeletedAt <= cutoffTime)
                .ToListAsync();

            if (pendingDeletes.Any())
            {
                _context.Customers.RemoveRange(pendingDeletes);
                await _context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Restore soft-deleted customer
        /// </summary>
        /// <param name="id">primary key of customer to restore.</param>
        /// <returns></returns>
        public async Task UndoDeleteCustomerAsync(int id)
        {
            // Retrieve the customer ignoring the global filter
            var customer = await _context.Customers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.CustomerId == id);

            if (customer != null && customer.IsDeleted)
            {
                // set delete flag to false
                customer.IsDeleted = false;
                // clear deletion time marker
                customer.DeletedAt = null;
                await _context.SaveChangesAsync();
            }

        }
    }
}
