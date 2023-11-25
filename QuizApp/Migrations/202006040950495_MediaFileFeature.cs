namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MediaFileFeature : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "EnableMediaFile", c => c.Boolean(nullable: false));
            AddColumn("dbo.BadgesInQuiz", "EnableMediaFile", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "EnableMediaFileForTitle", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "EnableMediaFileForDescription", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "EnableMediaFile", c => c.Boolean(nullable: false));
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "EnableMediaFile", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizResults", "EnableMediaFile", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "EnableMediaFile");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "EnableMediaFile");
            DropColumn("dbo.QuestionsInQuiz", "EnableMediaFile");
            DropColumn("dbo.ContentsInQuiz", "EnableMediaFileForDescription");
            DropColumn("dbo.ContentsInQuiz", "EnableMediaFileForTitle");
            DropColumn("dbo.BadgesInQuiz", "EnableMediaFile");
            DropColumn("dbo.QuizDetails", "EnableMediaFile");
        }
    }
}
