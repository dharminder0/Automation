namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_viewed : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizAttempts", "IsViewed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizAttempts", "IsViewed");
        }
    }
}
