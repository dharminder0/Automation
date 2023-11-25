namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_PreviousButtonFeature : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "ViewPreviousQuestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizDetails", "EditAnswer", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizDetails", "ApplyToAll", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "ViewPreviousQuestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "ViewPreviousQuestion", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "EditAnswer", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizObjectStats", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.QuizQuestionStats", "Status", c => c.Int(nullable: false));
            Sql("update QuizObjectStats set Status = 1");
            Sql("update QuizQuestionStats set Status = 1");
            Sql("update QuestionsInQuiz set RevealCorrectAnswer = 0 where AnswerType != 1 and AnswerType != 2");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizQuestionStats", "Status");
            DropColumn("dbo.QuizObjectStats", "Status");
            DropColumn("dbo.QuestionsInQuiz", "EditAnswer");
            DropColumn("dbo.QuestionsInQuiz", "ViewPreviousQuestion");
            DropColumn("dbo.ContentsInQuiz", "ViewPreviousQuestion");
            DropColumn("dbo.QuizDetails", "ApplyToAll");
            DropColumn("dbo.QuizDetails", "EditAnswer");
            DropColumn("dbo.QuizDetails", "ViewPreviousQuestion");
        }
    }
}
