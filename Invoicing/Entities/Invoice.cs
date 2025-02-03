/* Invoice.cs
* Invoicing App
* Liam Conn
* Handles the properties and navigation
* of invoice entity.
*
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Invoicing.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? InvoiceDueDate => InvoiceDate?
            .AddDays(PaymentTerms?.DueDays ?? 0); // invoice date + payment term days
        public double? PaymentTotal { get; set; } = 0.0;
        public DateTime? PaymentDate { get; set; }
        public int PaymentTermsId { get; set; }
        public int CustomerId { get; set; }

        // Navigation properties
        public Customer Customer { get; set; } = null!;
        public PaymentTerms PaymentTerms { get; set; } = null!;
        public ICollection<InvoiceLineItem> LineItems { get; set; } = new List<InvoiceLineItem>();
    }
}
