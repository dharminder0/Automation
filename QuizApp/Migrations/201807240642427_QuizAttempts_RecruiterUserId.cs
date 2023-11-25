namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizAttempts_RecruiterUserId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizAttempts", "RecruiterUserId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizAttempts", "RecruiterUserId");
        }
    }
}
