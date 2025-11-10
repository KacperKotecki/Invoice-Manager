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
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(20)]
        public string Name { get; set; }

        [Required]
        public decimal Rate { get; set; }

        [DefaultValue(false)]
        public bool IsDefault { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}