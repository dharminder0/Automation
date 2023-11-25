namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationTemplatesInQuiz_CompanyId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationTemplatesInQuiz", "CompanyId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationTemplatesInQuiz", "CompanyId");
        }
    }
}
