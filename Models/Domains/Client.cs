using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invoice_Manager.Models.Domains
{
    public class Client
    {
        public Client()
        {
            Invoices = new HashSet<Invoice>();
        }

        [Key]
        public int ClientId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(255)]
        public string ClientName { get; set; }

        [MaxLength(30)]
        public string TaxId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Street { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; }

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; }

        [Required]
        [MaxLength(100)]
        public string Country { get; set; }

        [MaxLength(255)]
        public string Email { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
    }
}