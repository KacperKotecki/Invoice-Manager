using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Invoice_Manager.Models.Domains
{
    public class Product
    {
        public Product()
        {
            InvoiceItems = new HashSet<InvoiceItem>();
        }

        [Key]
        public int ProductId { get; set; }

        [Required]
        public int CompanyId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(1000)]
        public string Description { get; set; }

        [Required]
        [MaxLength(20)]
        public string Unit { get; set; }

        [Required]
        public decimal UnitPriceNet { get; set; }

        [Required]
        public int DefaultTaxRateId { get; set; }

        [ForeignKey("CompanyId")]
        public virtual Company Company { get; set; }

        [ForeignKey("DefaultTaxRateId")]
        public virtual TaxRate DefaultTaxRate { get; set; }

        public virtual ICollection<InvoiceItem> InvoiceItems { get; set; }
    }
}