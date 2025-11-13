namespace Invoice_Manager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddIsProfileCompleteToCompany : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Companies", "IsProfileComplete", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Companies", "IsProfileComplete");
        }
    }
}
