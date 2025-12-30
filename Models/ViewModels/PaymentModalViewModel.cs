using Invoice_Manager.Models.Domains;
using System.Collections.Generic;

namespace Invoice_Manager.Models.ViewModels
{
    public class PaymentModalViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        
        public decimal TotalGrossAmount { get; set; }
        
        public decimal TotalPaid { get; set; }

        public List<Payment> Payments { get; set; }
        
        public decimal RemainingAmount 
        {
            get 
            { 
                return TotalGrossAmount - TotalPaid; 
            }
        }

        public bool IsFullyPaid 
        {
            get 
            { 
                return RemainingAmount <= 0; 
            }
        }
    }
}