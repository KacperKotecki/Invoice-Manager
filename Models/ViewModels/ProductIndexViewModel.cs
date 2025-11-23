using System.Collections.Generic;
using Invoice_Manager.Models.Domains;

namespace Invoice_Manager.Models.ViewModels
{
    public class ProductIndexViewModel
    {
        public IEnumerable<Product> Products { get; set; }
        public string SearchQuery { get; set; }
    }
}