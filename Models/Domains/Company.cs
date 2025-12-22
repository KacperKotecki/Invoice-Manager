using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invoice_Manager.Models.Domains
{
    public class Company
    {
        public Company()
        {
            Users = new HashSet<ApplicationUser>();
            Clients = new HashSet<Client>();
            Invoices = new HashSet<Invoice>();
            Products = new HashSet<Product>();
            TaxRates = new HashSet<TaxRate>();
        }

        [Key]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(255)]
        public string CompanyName { get; set; }

        [MaxLength(30)]
        public string TaxId { get; set; }

        [MaxLength(255)]
        public string Street { get; set; }

        [MaxLength(100)]
        public string City { get; set; }

        [MaxLength(20)]
        public string PostalCode { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(50)]
        public string BankName { get; set; }

        [MaxLength(50)]
        public string BankAccount { get; set; }

        [MaxLength(100)]
        public string Email { get; set; }

        [MaxLength(50)]
        public string Phone { get; set; }

        public bool IsProfileComplete { get; set; }

        public virtual ICollection<ApplicationUser> Users { get; set; } // w przypadku rozbudowy gdy do jednej firmy może należeć wielu użytkowników na ten moment raczej będzie tylko jedna osoba do jednej firmy 
        public virtual ICollection<Client> Clients { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<Product> Products { get; set; }
        public virtual ICollection<TaxRate> TaxRates { get; set; }
    }
}