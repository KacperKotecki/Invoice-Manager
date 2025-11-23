using System.Collections.Generic;
using Invoice_Manager.Models.Domains;

namespace Invoice_Manager.Models.ViewModels
{
    public class InvoiceFormViewModel
    {

        public string Heading { get; set; } 
        public Invoice Invoice { get; set; }
        public IEnumerable<Client> Clients { get; set; }
        public IEnumerable<Product> Products { get; set; }
        public IEnumerable<TaxRate> TaxRates { get; set; }
    }
}