using System.Collections.Generic;
using Invoice_Manager.Models.Domains;

namespace Invoice_Manager.Models.ViewModels
{
    public class ClientIndexViewModel
    {
        public IEnumerable<Client> Clients { get; set; }
        public string SearchQuery { get; set; }
    }
}