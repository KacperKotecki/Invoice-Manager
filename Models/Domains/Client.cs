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

        [Required(ErrorMessage = "Nazwa Klienta jest wymagana")]
        [MaxLength(255)]
        public string ClientName { get; set; }

        [MaxLength(30)]
        public string TaxId { get; set; }

        [Required(ErrorMessage = "Ulica jest wymagana")]
        [MaxLength(255)]
        public string Street { get; set; }

        [Required(ErrorMessage = "Miasto jest wymagane")]
        [MaxLength(100)]
        public string City { get; set; }

        [Required(ErrorMessage = "Kod pocztowy jest wymagany")]
        [MaxLength(20)]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Kraj jest wymagany")]
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