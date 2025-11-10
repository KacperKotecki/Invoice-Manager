using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Invoice_Manager.Models.Domains
{
    public class InvoiceItem
    {
        [Key]
        public int InvoiceItemId { get; set; }

        [Required]
        public int InvoiceId { get; set; }

        public int? ProductId { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [Required]
        public decimal Quantity { get; set; }

        [Required]
        [MaxLength(20)]
        public string Unit { get; set; }

        [Required]
        public decimal UnitPriceNet { get; set; }

        [Required]
        public decimal TaxRateValue { get; set; }

        [Required]
        public decimal TotalNetAmount { get; set; }

        [Required]
        public decimal TotalTaxAmount { get; set; }

        [Required]
        public decimal TotalGrossAmount { get; set; }

        [ForeignKey("InvoiceId")]
        public virtual Invoice Invoice { get; set; }

        [ForeignKey("ProductId")]
        public virtual Product Product { get; set; }
    }
}