namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationTemplate_WhatsApp : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationTemplate", "WhatsApp", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationTemplate", "WhatsApp");
        }
    }
}
