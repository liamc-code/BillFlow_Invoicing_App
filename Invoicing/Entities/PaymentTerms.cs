/* Customer.cs
* Invoicing App
* Liam Conn
* Handles the properties and validations methods
* of PaymentTerms entity.
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoicing.Entities
{
    public class PaymentTerms
    {
        public int PaymentTermsId { get; set; }
        public string Description { get; set; } = null!;
        public int DueDays { get; set; }

        // Navigation property
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
