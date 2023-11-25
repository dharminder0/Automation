namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_JAS_104_ResultNotificationTemplate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.NotificationTemplates", "OfficeId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.NotificationTemplates", "OfficeId", c => c.Int(nullable: false));
        }
    }
}
