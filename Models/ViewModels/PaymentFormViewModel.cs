using System;
using System.ComponentModel.DataAnnotations;

namespace Invoice_Manager.Models.ViewModels
{
    public class PaymentFormViewModel
    {
        [Required]
        public int InvoiceId { get; set; }

        public string InvoiceNumber { get; set; } 

        [Required]
        [Display(Name = "Data płatności")]
        [DataType(DataType.Date)] 
        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Kwota")]
        public decimal Amount { get; set; }

        [Display(Name = "Metoda płatności")]
        public string Method { get; set; } = "Przelew";
    }
}