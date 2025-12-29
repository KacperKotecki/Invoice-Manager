using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Invoice_Manager.Models.Common;

namespace Invoice_Manager.Models.Domains
{
    public class Invoice
    {
        public Invoice()
        {
            InvoiceItems = new HashSet<InvoiceItem>();
            Payments = new HashSet<Payment>();
        }

        [Key]
        public int InvoiceId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required(ErrorMessage = "Pole klient jest wymagane")]
        public int ClientId { get; set; }

        [Required]
        [MaxLength(100)]
        public string InvoiceNumber { get; set; }

        [Required(ErrorMessage = "Data wystawienia jest wymagana")]
        public DateTime IssueDate { get; set; }

        [Required(ErrorMessage = "Termin płatności jest wymagany")]
        public DateTime DueDate { get; set; }

        [Required(ErrorMessage = "Data sprzedaży jest wymagana")]
        public DateTime SaleDate { get; set; }

        [Required]
        public InvoiceStatus Status { get; set; }

        // Pola Danych (Snapshot Sprzedawcy)
        [Required]
        [MaxLength(255)]
        public string Company_Name { get; set; }
        [Required]
        [MaxLength(30)]
        public string Company_TaxId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Company_Street { get; set; }
        [Required]
        [MaxLength(100)]
        public string Company_City { get; set; }
        [Required]
        [MaxLength(20)]
        public string Company_PostalCode { get; set; }

        [Required]
        [MaxLength(50)]
        public string Company_BankName { get; set; }

        [Required]
        [MaxLength(50)]
        public string Company_BankAccount { get; set; }

        // Pola Danych (Snapshot Nabywcy)
        [Required]
        [MaxLength(255)]
        public string Client_Name { get; set; }
        [MaxLength(30)]
        public string Client_TaxId { get; set; }
        [Required]
        [MaxLength(255)]
        public string Client_Street { get; set; }
        [Required]
        [MaxLength(100)]
        public string Client_City { get; set; }
        [Required]
        [MaxLength(20)]
        public string Client_PostalCode { get; set; }


        [Required]
        public decimal TotalNetAmount { get; set; }

        [Required]
        public decimal TotalTaxAmount { get; set; }

        [Required]
        public decimal TotalGrossAmount { get; set; }

        [NotMapped]
        public string TotalAmountInWords
        {
            get
            {
                return AmountToWordsHelper.ConvertToWords(TotalGrossAmount);
            }
            
        }

        [Required(ErrorMessage = "Waluta jest wymagana")]
        [MaxLength(5)]
        public string Currency { get; set; }

        [Required(ErrorMessage = "Metoda płatności jest wymagana")]
        [MaxLength(100)]
        public string PaymentMethod { get; set; }

        [MaxLength(2000)]
        public string Notes { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("ClientId")]
        public virtual Client Client { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
        public virtual ICollection<Payment> Payments { get; set; }


       
    }
}