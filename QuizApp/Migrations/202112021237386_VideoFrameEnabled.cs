namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VideoFrameEnabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "VideoFrameEnabled", c => c.Boolean());
            AddColumn("dbo.BadgesInQuiz", "VideoFrameEnabled", c => c.Boolean());
            AddColumn("dbo.ContentsInQuiz", "VideoFrameEnabled", c => c.Boolean());
            AddColumn("dbo.ContentsInQuiz", "DescVideoFrameEnabled", c => c.Boolean());
            AddColumn("dbo.QuestionsInQuiz", "VideoFrameEnabled", c => c.Boolean());
            AddColumn("dbo.QuestionsInQuiz", "DescVideoFrameEnabled", c => c.Boolean());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "VideoFrameEnabled", c => c.Boolean());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "DescVideoFrameEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizResults", "VideoFrameEnabled", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "VideoFrameEnabled");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "DescVideoFrameEnabled");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "VideoFrameEnabled");
            DropColumn("dbo.QuestionsInQuiz", "DescVideoFrameEnabled");
            DropColumn("dbo.QuestionsInQuiz", "VideoFrameEnabled");
            DropColumn("dbo.ContentsInQuiz", "DescVideoFrameEnabled");
            DropColumn("dbo.ContentsInQuiz", "VideoFrameEnabled");
            DropColumn("dbo.BadgesInQuiz", "VideoFrameEnabled");
            DropColumn("dbo.QuizDetails", "VideoFrameEnabled");
        }
    }
}
