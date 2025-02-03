/* ICustomerService.cs
* Invoicing App
* Liam Conn
* Interface for defining Tasks to pass onto CustomerService.
* Includes operations for managing, retrieval, creation,
* updating, and deletion of customers.
*
*/

using Invoicing.Entities;

namespace Invoicing.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<Customer>> GetCustomersByGroupAsync(string group);
        Task<Customer?> GetCustomerByIdAsync(int id);
        Task AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task SoftDeleteCustomerAsync(int id);
        Task HardDeleteCustomerAsync(int id);
        Task UndoDeleteCustomerAsync(int id);
        Task CleanupPendingDeletes();

    }
}
