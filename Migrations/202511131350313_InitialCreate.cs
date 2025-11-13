namespace Invoice_Manager.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Clients",
                c => new
                    {
                        ClientId = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        ClientName = c.String(nullable: false, maxLength: 255),
                        TaxId = c.String(maxLength: 30),
                        Street = c.String(nullable: false, maxLength: 255),
                        City = c.String(nullable: false, maxLength: 100),
                        PostalCode = c.String(nullable: false, maxLength: 20),
                        Country = c.String(nullable: false, maxLength: 100),
                        Email = c.String(maxLength: 255),
                        IsActive = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.ClientId)
                .ForeignKey("dbo.Companies", t => t.CompanyId, cascadeDelete: true)
                .Index(t => t.CompanyId);
            
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        CompanyId = c.Int(nullable: false, identity: true),
                        CompanyName = c.String(nullable: false, maxLength: 255),
                        TaxId = c.String(nullable: false, maxLength: 30),
                        Street = c.String(nullable: false, maxLength: 255),
                        City = c.String(nullable: false, maxLength: 100),
                        PostalCode = c.String(nullable: false, maxLength: 20),
                        Country = c.String(nullable: false, maxLength: 100),
                        BankAccount = c.String(nullable: false, maxLength: 50),
                        Email = c.String(maxLength: 100),
                        Phone = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.CompanyId);
            
            CreateTable(
                "dbo.Invoices",
                c => new
                    {
                        InvoiceId = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        ClientId = c.Int(nullable: false),
                        InvoiceNumber = c.String(nullable: false, maxLength: 100),
                        IssueDate = c.DateTime(nullable: false),
                        DueDate = c.DateTime(nullable: false),
                        SaleDate = c.DateTime(nullable: false),
                        Status = c.Int(nullable: false),
                        Company_Name = c.String(nullable: false, maxLength: 255),
                        Company_TaxId = c.String(nullable: false, maxLength: 30),
                        Company_Street = c.String(nullable: false, maxLength: 255),
                        Company_City = c.String(nullable: false, maxLength: 100),
                        Company_PostalCode = c.String(nullable: false, maxLength: 20),
                        Company_BankAccount = c.String(nullable: false, maxLength: 50),
                        Client_Name = c.String(nullable: false, maxLength: 255),
                        Client_TaxId = c.String(maxLength: 30),
                        Client_Street = c.String(nullable: false, maxLength: 255),
                        Client_City = c.String(nullable: false, maxLength: 100),
                        Client_PostalCode = c.String(nullable: false, maxLength: 20),
                        TotalNetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalTaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalGrossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Currency = c.String(nullable: false, maxLength: 5),
                        PaymentMethod = c.String(nullable: false, maxLength: 100),
                        Notes = c.String(maxLength: 2000),
                    })
                .PrimaryKey(t => t.InvoiceId)
                .ForeignKey("dbo.Companies", t => t.CompanyId)
                .ForeignKey("dbo.Clients", t => t.ClientId)
                .Index(t => t.CompanyId)
                .Index(t => t.ClientId);
            
            CreateTable(
                "dbo.InvoiceItems",
                c => new
                    {
                        InvoiceItemId = c.Int(nullable: false, identity: true),
                        InvoiceId = c.Int(nullable: false),
                        ProductId = c.Int(),
                        Name = c.String(nullable: false, maxLength: 255),
                        Quantity = c.Decimal(nullable: false, precision: 18, scale: 4),
                        Unit = c.String(nullable: false, maxLength: 20),
                        UnitPriceNet = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TaxRateValue = c.Decimal(nullable: false, precision: 5, scale: 2),
                        TotalNetAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalTaxAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        TotalGrossAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.InvoiceItemId)
                .ForeignKey("dbo.Invoices", t => t.InvoiceId, cascadeDelete: true)
                .ForeignKey("dbo.Products", t => t.ProductId)
                .Index(t => t.InvoiceId)
                .Index(t => t.ProductId);
            
            CreateTable(
                "dbo.Products",
                c => new
                    {
                        ProductId = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 255),
                        Description = c.String(maxLength: 1000),
                        Unit = c.String(nullable: false, maxLength: 20),
                        UnitPriceNet = c.Decimal(nullable: false, precision: 18, scale: 2),
                        DefaultTaxRateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ProductId)
                .ForeignKey("dbo.Companies", t => t.CompanyId, cascadeDelete: true)
                .ForeignKey("dbo.TaxRates", t => t.DefaultTaxRateId)
                .Index(t => t.CompanyId)
                .Index(t => t.DefaultTaxRateId);
            
            CreateTable(
                "dbo.TaxRates",
                c => new
                    {
                        TaxRateId = c.Int(nullable: false, identity: true),
                        CompanyId = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 20),
                        Rate = c.Decimal(nullable: false, precision: 5, scale: 2),
                        IsDefault = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.TaxRateId)
                .ForeignKey("dbo.Companies", t => t.CompanyId, cascadeDelete: true)
                .Index(t => t.CompanyId);
            
            CreateTable(
                "dbo.Payments",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        InvoiceId = c.Int(nullable: false),
                        PaymentDate = c.DateTime(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Method = c.String(maxLength: 100),
                        TransactionId = c.String(maxLength: 255),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Invoices", t => t.InvoiceId, cascadeDelete: true)
                .Index(t => t.InvoiceId);
            
            CreateTable(
                "dbo.AspNetUsers",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        CompanyId = c.Int(nullable: false),
                        Email = c.String(maxLength: 256),
                        EmailConfirmed = c.Boolean(nullable: false),
                        PasswordHash = c.String(),
                        SecurityStamp = c.String(),
                        PhoneNumber = c.String(),
                        PhoneNumberConfirmed = c.Boolean(nullable: false),
                        TwoFactorEnabled = c.Boolean(nullable: false),
                        LockoutEndDateUtc = c.DateTime(),
                        LockoutEnabled = c.Boolean(nullable: false),
                        AccessFailedCount = c.Int(nullable: false),
                        UserName = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId, cascadeDelete: true)
                .Index(t => t.CompanyId)
                .Index(t => t.UserName, unique: true, name: "UserNameIndex");
            
            CreateTable(
                "dbo.AspNetUserClaims",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserId = c.String(nullable: false, maxLength: 128),
                        ClaimType = c.String(),
                        ClaimValue = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserLogins",
                c => new
                    {
                        LoginProvider = c.String(nullable: false, maxLength: 128),
                        ProviderKey = c.String(nullable: false, maxLength: 128),
                        UserId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.LoginProvider, t.ProviderKey, t.UserId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId);
            
            CreateTable(
                "dbo.AspNetUserRoles",
                c => new
                    {
                        UserId = c.String(nullable: false, maxLength: 128),
                        RoleId = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => new { t.UserId, t.RoleId })
                .ForeignKey("dbo.AspNetUsers", t => t.UserId, cascadeDelete: true)
                .ForeignKey("dbo.AspNetRoles", t => t.RoleId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.RoleId);
            
            CreateTable(
                "dbo.AspNetRoles",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        Name = c.String(nullable: false, maxLength: 256),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true, name: "RoleNameIndex");
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AspNetUserRoles", "RoleId", "dbo.AspNetRoles");
            DropForeignKey("dbo.Invoices", "ClientId", "dbo.Clients");
            DropForeignKey("dbo.AspNetUserRoles", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUserLogins", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.AspNetUsers", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.AspNetUserClaims", "UserId", "dbo.AspNetUsers");
            DropForeignKey("dbo.Invoices", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.Payments", "InvoiceId", "dbo.Invoices");
            DropForeignKey("dbo.InvoiceItems", "ProductId", "dbo.Products");
            DropForeignKey("dbo.Products", "DefaultTaxRateId", "dbo.TaxRates");
            DropForeignKey("dbo.TaxRates", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.Products", "CompanyId", "dbo.Companies");
            DropForeignKey("dbo.InvoiceItems", "InvoiceId", "dbo.Invoices");
            DropForeignKey("dbo.Clients", "CompanyId", "dbo.Companies");
            DropIndex("dbo.AspNetRoles", "RoleNameIndex");
            DropIndex("dbo.AspNetUserRoles", new[] { "RoleId" });
            DropIndex("dbo.AspNetUserRoles", new[] { "UserId" });
            DropIndex("dbo.AspNetUserLogins", new[] { "UserId" });
            DropIndex("dbo.AspNetUserClaims", new[] { "UserId" });
            DropIndex("dbo.AspNetUsers", "UserNameIndex");
            DropIndex("dbo.AspNetUsers", new[] { "CompanyId" });
            DropIndex("dbo.Payments", new[] { "InvoiceId" });
            DropIndex("dbo.TaxRates", new[] { "CompanyId" });
            DropIndex("dbo.Products", new[] { "DefaultTaxRateId" });
            DropIndex("dbo.Products", new[] { "CompanyId" });
            DropIndex("dbo.InvoiceItems", new[] { "ProductId" });
            DropIndex("dbo.InvoiceItems", new[] { "InvoiceId" });
            DropIndex("dbo.Invoices", new[] { "ClientId" });
            DropIndex("dbo.Invoices", new[] { "CompanyId" });
            DropIndex("dbo.Clients", new[] { "CompanyId" });
            DropTable("dbo.AspNetRoles");
            DropTable("dbo.AspNetUserRoles");
            DropTable("dbo.AspNetUserLogins");
            DropTable("dbo.AspNetUserClaims");
            DropTable("dbo.AspNetUsers");
            DropTable("dbo.Payments");
            DropTable("dbo.TaxRates");
            DropTable("dbo.Products");
            DropTable("dbo.InvoiceItems");
            DropTable("dbo.Invoices");
            DropTable("dbo.Companies");
            DropTable("dbo.Clients");
        }
    }
}
