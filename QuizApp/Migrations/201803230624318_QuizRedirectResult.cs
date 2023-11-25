namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizRedirectResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizResults", "IsRedirectOn", c => c.Boolean());
            AddColumn("dbo.QuizResults", "RedirectResultTo", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "RedirectResultTo");
            DropColumn("dbo.QuizResults", "IsRedirectOn");
        }
    }
}
