namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationTemplate_CompanyId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationTemplate", "CompanyId", c => c.Int());
            AddColumn("dbo.QuizAttempts", "CompanyId", c => c.Int());
            CreateIndex("dbo.NotificationTemplate", "CompanyId");
            CreateIndex("dbo.QuizAttempts", "CompanyId");
            AddForeignKey("dbo.QuizAttempts", "CompanyId", "dbo.Company", "Id");
            AddForeignKey("dbo.NotificationTemplate", "CompanyId", "dbo.Company", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.NotificationTemplate", "CompanyId", "dbo.Company");
            DropForeignKey("dbo.QuizAttempts", "CompanyId", "dbo.Company");
            DropIndex("dbo.QuizAttempts", new[] { "CompanyId" });
            DropIndex("dbo.NotificationTemplate", new[] { "CompanyId" });
            DropColumn("dbo.QuizAttempts", "CompanyId");
            DropColumn("dbo.NotificationTemplate", "CompanyId");
        }
    }
}
