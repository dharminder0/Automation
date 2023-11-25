namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Template_Title : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationTemplate", "Title", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.NotificationTemplate", "Title");
        }
    }
}
