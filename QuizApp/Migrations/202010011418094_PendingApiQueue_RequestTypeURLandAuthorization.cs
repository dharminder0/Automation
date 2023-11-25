namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PendingApiQueue_RequestTypeURLandAuthorization : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.PendingApiQueue", "CompanyId", "dbo.Company");
            DropIndex("dbo.PendingApiQueue", new[] { "CompanyId" });
            AddColumn("dbo.PendingApiQueue", "RequestTypeURL", c => c.String());
            AddColumn("dbo.PendingApiQueue", "Authorization", c => c.String());
            DropColumn("dbo.PendingApiQueue", "CompanyId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.PendingApiQueue", "CompanyId", c => c.Int());
            DropColumn("dbo.PendingApiQueue", "Authorization");
            DropColumn("dbo.PendingApiQueue", "RequestTypeURL");
            CreateIndex("dbo.PendingApiQueue", "CompanyId");
            AddForeignKey("dbo.PendingApiQueue", "CompanyId", "dbo.Company", "Id");
        }
    }
}
