namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationTemplateUpdateForPublish : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationTemplate", "State", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationTemplate", "State");
        }
    }
}
