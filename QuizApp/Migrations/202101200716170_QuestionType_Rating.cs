namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionType_Rating : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuiz", "EnableComment", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "TopicTitle", c => c.String());
            AddColumn("dbo.QuizAnswerStats", "Comment", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizAnswerStats", "Comment");
            DropColumn("dbo.QuestionsInQuiz", "TopicTitle");
            DropColumn("dbo.QuestionsInQuiz", "EnableComment");
        }
    }
}
