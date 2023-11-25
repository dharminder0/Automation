namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionsInQuiz_IsMultiRating : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuiz", "IsMultiRating", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuiz", "IsMultiRating");
        }
    }
}
