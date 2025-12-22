namespace Invoice_Manager.Migrations
{
    using Invoice_Manager.Models.Domains;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Invoice_Manager.Models.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Invoice_Manager.Models.ApplicationDbContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method
            //  to avoid creating duplicate seed data.

            context.TaxRates.AddOrUpdate(
                x => new { x.Country, x.Rate, x.Name }, // Klucz unikalności (żeby nie dublować przy każdym update-database)

                // POLSKA (PL)
                new TaxRate { Name = "VAT 23%", Rate = 23.00m, Country = "PL", IsActive = true},
                new TaxRate { Name = "VAT 8%", Rate = 8.00m, Country = "PL", IsActive = true},
                new TaxRate { Name = "VAT 5%", Rate = 5.00m, Country = "PL", IsActive = true},
                new TaxRate { Name = "VAT 0%", Rate = 0.00m, Country = "PL", IsActive = true},
                new TaxRate { Name = "ZW", Rate = 0.00m, Country = "PL", IsActive = true},

                // NIEMCY (DE) - przykładowo
                new TaxRate { Name = "MwSt 19%", Rate = 19.00m, Country = "DE", IsActive = true},
                new TaxRate { Name = "MwSt 7%", Rate = 7.00m, Country = "DE", IsActive = true},

                // WIELKA BRYTANIA (GB)
                new TaxRate { Name = "VAT 20%", Rate = 20.00m, Country = "GB", IsActive = true},
                new TaxRate { Name = "VAT 5%", Rate = 5.00m, Country = "GB", IsActive = true}
            );
        }
    }
}
