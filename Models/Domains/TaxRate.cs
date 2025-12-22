using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invoice_Manager.Models.Domains
{
    public class TaxRate
    {
        public TaxRate()
        {
            Products = new HashSet<Product>();
        }

        [Key]
        public int TaxRateId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        public decimal Rate { get; set; }

        [Required]
        [MaxLength(2)]
        public string Country { get; set; }

        [DefaultValue(true)]
        public bool IsActive { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}