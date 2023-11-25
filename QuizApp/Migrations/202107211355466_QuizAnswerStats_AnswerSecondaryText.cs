namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizAnswerStats_AnswerSecondaryText : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizAnswerStats", "AnswerSecondaryText", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizAnswerStats", "AnswerSecondaryText");
        }
    }
}
