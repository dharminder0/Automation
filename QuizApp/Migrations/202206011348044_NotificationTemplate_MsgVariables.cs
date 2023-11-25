namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationTemplate_MsgVariables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationTemplate", "MsgVariables", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationTemplate", "MsgVariables");
        }
    }
}
