namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizAttempts_SourceId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizAttempts", "SourceId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizAttempts", "SourceId");
        }
    }
}
