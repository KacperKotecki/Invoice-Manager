using System.ComponentModel.DataAnnotations;

namespace Invoice_Manager.Models.ViewModels
{
    public class CompleteProfileViewModel
    {
        [Required(ErrorMessage = "Nazwa firmy jest wymagana.")]
        [MaxLength(255)]
        [Display(Name = "Pe³na nazwa firmy")]
        public string CompanyName { get; set; }

        [Required(ErrorMessage = "NIP jest wymagany.")]
        [MaxLength(30)]
        [Display(Name = "NIP")]
        public string TaxId { get; set; }

        [Required(ErrorMessage = "Ulica jest wymagana.")]
        [MaxLength(255)]
        [Display(Name = "Ulica i numer")]
        public string Street { get; set; }

        [Required(ErrorMessage = "Miasto jest wymagane.")]
        [MaxLength(100)]
        [Display(Name = "Miasto")]
        public string City { get; set; }

        [Required(ErrorMessage = "Kod pocztowy jest wymagany.")]
        [MaxLength(20)]
        [Display(Name = "Kod pocztowy")]
        public string PostalCode { get; set; }

        [Required(ErrorMessage = "Kraj jest wymagany.")]
        [MaxLength(100)]
        [Display(Name = "Kraj")]
        public string Country { get; set; }

        [Required(ErrorMessage = "Numer konta bankowego jest wymagany.")]
        [MaxLength(50)]
        [Display(Name = "Numer konta bankowego")]
        public string BankAccount { get; set; }

        [MaxLength(100)]
        [EmailAddress]
        [Display(Name = "Adres e-mail firmy")]
        public string Email { get; set; }

        [MaxLength(50)]
        [Display(Name = "Telefon")]
        public string Phone { get; set; }
    }
}