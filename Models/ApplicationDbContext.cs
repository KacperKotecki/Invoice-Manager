using Invoice_Manager.Models.Domains;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;

namespace Invoice_Manager.Models
{

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceItem> InvoiceItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<TaxRate> TaxRates { get; set; }


        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Konfiguracja precyzji dla pól decimal
            modelBuilder.Entity<TaxRate>().Property(x => x.Rate).HasPrecision(5, 2);
            modelBuilder.Entity<Product>().Property(x => x.UnitPriceNet).HasPrecision(18, 2);

            modelBuilder.Entity<Invoice>().Property(x => x.TotalNetAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>().Property(x => x.TotalTaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<Invoice>().Property(x => x.TotalGrossAmount).HasPrecision(18, 2);

            modelBuilder.Entity<InvoiceItem>().Property(x => x.Quantity).HasPrecision(18, 4);
            modelBuilder.Entity<InvoiceItem>().Property(x => x.UnitPriceNet).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>().Property(x => x.TaxRateValue).HasPrecision(5, 2);
            modelBuilder.Entity<InvoiceItem>().Property(x => x.TotalNetAmount).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>().Property(x => x.TotalTaxAmount).HasPrecision(18, 2);
            modelBuilder.Entity<InvoiceItem>().Property(x => x.TotalGrossAmount).HasPrecision(18, 2);

            modelBuilder.Entity<Payment>().Property(x => x.Amount).HasPrecision(18, 2);

            // Konfiguracja relacji kaskadowego usuwania
            modelBuilder.Entity<Company>()
                .HasMany(c => c.Invoices)
                .WithRequired(i => i.Company)
                .HasForeignKey(i => i.CompanyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.Invoices)
                .WithRequired(i => i.Client)
                .HasForeignKey(i => i.ClientId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.InvoiceItems)
                .WithOptional(ii => ii.Product)
                .HasForeignKey(ii => ii.ProductId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TaxRate>()
                .HasMany(t => t.Products)
                .WithRequired(p => p.DefaultTaxRate)
                .HasForeignKey(p => p.DefaultTaxRateId)
                .WillCascadeOnDelete(false);
        }
    }
}