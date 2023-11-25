namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizAttempt_JAS_124 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.QuizAttempts", "BusinessUserId");
            DropColumn("dbo.QuizAttempts", "CreatedBy");
            DropColumn("dbo.QuizAttempts", "LastUpdatedBy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizAttempts", "LastUpdatedBy", c => c.Int());
            AddColumn("dbo.QuizAttempts", "CreatedBy", c => c.Int(nullable: false));
            AddColumn("dbo.QuizAttempts", "BusinessUserId", c => c.Int());
        }
    }
}
