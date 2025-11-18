using Invoice_Manager.Models.Domains;
using System.Collections.Generic;

namespace Invoice_Manager.Models.ViewModels
{
    public class DashboardViewModel
    {
        public List<InvoiceCardViewModel> Invoices { get; set; }
        public DashboardStatsViewModel Stats { get; set; }
        public string SearchQuery { get; set; }
        public InvoiceStatus? ActiveFilter { get; set; }
    }
}
