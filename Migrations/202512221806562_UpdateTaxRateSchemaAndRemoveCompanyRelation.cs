namespace Invoice_Manager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UpdateTaxRateSchemaAndRemoveCompanyRelation : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TaxRates", "CompanyId", "dbo.Companies");
            DropIndex("dbo.TaxRates", new[] { "CompanyId" });
            RenameColumn(table: "dbo.TaxRates", name: "CompanyId", newName: "Company_CompanyId");
            AddColumn("dbo.TaxRates", "Country", c => c.String(nullable: false, maxLength: 2));
            AddColumn("dbo.TaxRates", "IsActive", c => c.Boolean(nullable: false));
            AlterColumn("dbo.TaxRates", "Company_CompanyId", c => c.Int());
            CreateIndex("dbo.TaxRates", "Company_CompanyId");
            AddForeignKey("dbo.TaxRates", "Company_CompanyId", "dbo.Companies", "CompanyId");
            DropColumn("dbo.TaxRates", "IsDefault");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TaxRates", "IsDefault", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.TaxRates", "Company_CompanyId", "dbo.Companies");
            DropIndex("dbo.TaxRates", new[] { "Company_CompanyId" });
            AlterColumn("dbo.TaxRates", "Company_CompanyId", c => c.Int(nullable: false));
            DropColumn("dbo.TaxRates", "IsActive");
            DropColumn("dbo.TaxRates", "Country");
            RenameColumn(table: "dbo.TaxRates", name: "Company_CompanyId", newName: "CompanyId");
            CreateIndex("dbo.TaxRates", "CompanyId");
            AddForeignKey("dbo.TaxRates", "CompanyId", "dbo.Companies", "CompanyId", cascadeDelete: true);
        }
    }
}
