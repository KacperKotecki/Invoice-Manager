namespace Invoice_Manager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBankNameRequiredToCompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Invoices", "Company_BankName", c => c.String(nullable: false, maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Invoices", "Company_BankName");
        }
    }
}
