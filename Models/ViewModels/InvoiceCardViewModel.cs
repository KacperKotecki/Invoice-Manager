using Invoice_Manager.Models.Domains;
using System;

namespace Invoice_Manager.Models.ViewModels
{
    public class InvoiceCardViewModel
    {
        public int InvoiceId { get; set; }
        public string InvoiceNumber { get; set; }
        public string ClientName { get; set; }
        public decimal TotalGrossAmount { get; set; }
        public string Currency { get; set; }
        public InvoiceStatus Status { get; set; }
        public DateTime DueDate { get; set; }
    }
}
