namespace Invoice_Manager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveTaxRateCollectionFromCompany : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaxRates", "Company_CompanyId", "dbo.Companies");
            DropIndex("dbo.TaxRates", new[] { "Company_CompanyId" });
            DropColumn("dbo.TaxRates", "Company_CompanyId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaxRates", "Company_CompanyId", c => c.Int());
            CreateIndex("dbo.TaxRates", "Company_CompanyId");
            AddForeignKey("dbo.TaxRates", "Company_CompanyId", "dbo.Companies", "CompanyId");
        }
    }
}
