namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_RelatedTables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "ShowAnswerImage", c => c.Boolean());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "DisplayOrder", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionsInQuizs", "ShowQuestionImage", c => c.Boolean());
            AddColumn("dbo.QuestionsInQuizs", "LastUpdatedOn", c => c.DateTime());
            AddColumn("dbo.QuestionsInQuizs", "LastUpdatedBy", c => c.Int());
            AddColumn("dbo.QuestionsInQuizs", "DisplayOrder", c => c.Int(nullable: false));
            AddColumn("dbo.Quizs", "QuizCoverImgXCoordinate", c => c.Int());
            AddColumn("dbo.Quizs", "QuizCoverImgYCoordinate", c => c.Int());
            AddColumn("dbo.Quizs", "QuizCoverImgHeight", c => c.Int());
            AddColumn("dbo.Quizs", "QuizCoverImgWidth", c => c.Int());
            AddColumn("dbo.Quizs", "IsBranchingLogicEnabled", c => c.Boolean());
            AddColumn("dbo.Quizs", "HideSocialShareButtons", c => c.Boolean());
            AddColumn("dbo.QuizResults", "HideCallToAction", c => c.Boolean());
            AddColumn("dbo.QuizResults", "ActionButtonText", c => c.String(maxLength: 50));
            DropColumn("dbo.QuizResults", "ActionButtonButtonColor");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizResults", "ActionButtonButtonColor", c => c.String(maxLength: 50));
            DropColumn("dbo.QuizResults", "ActionButtonText");
            DropColumn("dbo.QuizResults", "HideCallToAction");
            DropColumn("dbo.Quizs", "HideSocialShareButtons");
            DropColumn("dbo.Quizs", "IsBranchingLogicEnabled");
            DropColumn("dbo.Quizs", "QuizCoverImgWidth");
            DropColumn("dbo.Quizs", "QuizCoverImgHeight");
            DropColumn("dbo.Quizs", "QuizCoverImgYCoordinate");
            DropColumn("dbo.Quizs", "QuizCoverImgXCoordinate");
            DropColumn("dbo.QuestionsInQuizs", "DisplayOrder");
            DropColumn("dbo.QuestionsInQuizs", "LastUpdatedBy");
            DropColumn("dbo.QuestionsInQuizs", "LastUpdatedOn");
            DropColumn("dbo.QuestionsInQuizs", "ShowQuestionImage");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "DisplayOrder");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "ShowAnswerImage");
        }
    }
}
