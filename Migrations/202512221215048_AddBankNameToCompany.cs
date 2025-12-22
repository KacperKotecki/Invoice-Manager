namespace Invoice_Manager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddBankNameToCompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "BankName", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "BankName");
        }
    }
}
