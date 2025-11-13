namespace Invoice_Manager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeCompanyDetailsOptional : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.Companies", "TaxId", c => c.String(maxLength: 30));
            AlterColumn("dbo.Companies", "Street", c => c.String(maxLength: 255));
            AlterColumn("dbo.Companies", "City", c => c.String(maxLength: 100));
            AlterColumn("dbo.Companies", "PostalCode", c => c.String(maxLength: 20));
            AlterColumn("dbo.Companies", "Country", c => c.String(maxLength: 100));
            AlterColumn("dbo.Companies", "BankAccount", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.Companies", "BankAccount", c => c.String(nullable: false, maxLength: 50));
            AlterColumn("dbo.Companies", "Country", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Companies", "PostalCode", c => c.String(nullable: false, maxLength: 20));
            AlterColumn("dbo.Companies", "City", c => c.String(nullable: false, maxLength: 100));
            AlterColumn("dbo.Companies", "Street", c => c.String(nullable: false, maxLength: 255));
            AlterColumn("dbo.Companies", "TaxId", c => c.String(nullable: false, maxLength: 30));
        }
    }
}
