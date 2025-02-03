/* Customer.cs
* Invoicing App
* Liam Conn
* Handles the properties and validations methods
* of customer entity.
*
*/

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Invoicing.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = null!;

        [Required(ErrorMessage = "Address Line 1 is required.")]
        public string? Address1 { get; set; }
        public string? Address2 { get; set; }

        [Required(ErrorMessage = "City is required.")]
        public string? City { get; set; } = null!;

        // validation for 2 letter (upper or lower case) input
        [Required(ErrorMessage = "Province/State is required.")]
        [RegularExpression(@"^[A-Za-z]{2}$", ErrorMessage = "Province/State must be a 2-letter code.")]
        public string? ProvinceOrState { get; set; } = null!;

        // Canada/US specific Zip/Postal code validation on input
        [Required(ErrorMessage = "Zip/Postal Code is required.")]
        [CustomValidation(typeof(Customer), nameof(ValidateZipOrPostalCode))]
        public string? ZipOrPostalCode { get; set; } = null!;

        // US/Canada specific phone validation on input
        [Required(ErrorMessage = "Phone number is required.")]
        [CustomValidation(typeof(Customer), nameof(ValidatePhoneNumber))]
        public string? Phone { get; set; }
        public string? ContactLastName { get; set; }
        public string? ContactFirstName { get; set; }

        [EmailAddress(ErrorMessage = "The Contact Email is not in a valid format.")]
        public string? ContactEmail { get; set; }
        public bool IsDeleted { get; set; } = false; // flag for db deletion
        public DateTime? DeletedAt { get; set; } // Nullable DateTime for soft-delete timestamp

        // Navigation property
        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();


        /// <summary>
        /// Custom validation for US/Canadian Zip/Postal Codes
        /// </summary>
        /// <param name="zipOrPostalCode"></param>
        /// <param name="context"></param>
        /// <returns>Passes validation or not</returns>
        public static ValidationResult? ValidateZipOrPostalCode(string zipOrPostalCode, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(zipOrPostalCode))
            {
                return new ValidationResult("Zip/Postal Code is required.");
            }

            var usZipRegex = @"^\d{5}(-\d{4})?$"; // US Zip Code (e.g., 12345 or 12345-6789)
            var caPostalRegex = @"^[A-Za-z]\d[A-Za-z] ?\d[A-Za-z]\d$"; // Canadian Postal Code (e.g., A1A 1A1)

            if (!Regex.IsMatch(zipOrPostalCode, usZipRegex) && !Regex.IsMatch(zipOrPostalCode, caPostalRegex))
            {
                return new ValidationResult("The Zip/Postal Code must be in a valid US or Canadian format.");
            }

            return ValidationResult.Success;
        }

        /// <summary>
        /// Custom validation for US/Canadian Phone Numbers
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="context"></param>
        /// <returns>Passes validation or not</returns>
        public static ValidationResult? ValidatePhoneNumber(string phone, ValidationContext context)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return new ValidationResult("Phone number is required.");
            }

            // Updated regex to handle US/Canadian formats
            var phoneRegex = @"^(\+1\s?)?(\(?\d{3}\)?[-.\s]?)\d{3}[-.\s]?\d{4}$";

            if (!Regex.IsMatch(phone, phoneRegex))
            {
                return new ValidationResult("The Phone number must be in a valid US or Canadian format.");
            }

            return ValidationResult.Success;
        }
    }
}

