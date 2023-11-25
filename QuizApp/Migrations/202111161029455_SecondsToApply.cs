namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class SecondsToApply : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BadgesInQuiz", "SecondsToApply", c => c.String());
            AddColumn("dbo.ContentsInQuiz", "SecondsToApply", c => c.String());
            AddColumn("dbo.ContentsInQuiz", "SecondsToApplyForDescription", c => c.String());
            AddColumn("dbo.QuestionsInQuiz", "SecondsToApply", c => c.String());
            AddColumn("dbo.QuestionsInQuiz", "SecondsToApplyForDescription", c => c.String());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "SecondsToApply", c => c.String());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "SecondsToApplyForDescription", c => c.String());
            AddColumn("dbo.QuizResults", "SecondsToApply", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "SecondsToApply");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "SecondsToApplyForDescription");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "SecondsToApply");
            DropColumn("dbo.QuestionsInQuiz", "SecondsToApplyForDescription");
            DropColumn("dbo.QuestionsInQuiz", "SecondsToApply");
            DropColumn("dbo.ContentsInQuiz", "SecondsToApplyForDescription");
            DropColumn("dbo.ContentsInQuiz", "SecondsToApply");
            DropColumn("dbo.BadgesInQuiz", "SecondsToApply");
        }
    }
}
