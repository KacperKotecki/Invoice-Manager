using System.ComponentModel.DataAnnotations;

namespace Invoice_Manager.Models
{
    public class ForgotViewModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }
}
