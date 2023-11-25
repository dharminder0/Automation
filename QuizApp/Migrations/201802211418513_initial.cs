namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AnswerOptionsInQuizQuestions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuestionId = c.Int(nullable: false),
                        Option = c.String(maxLength: 2000),
                        OptionImage = c.String(maxLength: 2000),
                        AssociatedScore = c.Int(),
                        IsCorrectAnswer = c.Boolean(),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuestionsInQuizs", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId);
            
            CreateTable(
                "dbo.QuestionsInQuizs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        Question = c.String(),
                        QuestionImage = c.String(),
                        SetCorrectAnswer = c.Boolean(),
                        CorrectAnswerDescription = c.String(),
                        RevealCorrectAnswer = c.Boolean(),
                        AliasTextForCorrect = c.String(),
                        AliasTextForIncorrect = c.String(),
                        AliasTextForYourAnswer = c.String(),
                        AliasTextForCorrectAnswer = c.String(),
                        AliasTextForExplanation = c.String(),
                        AliasTextForNextButton = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quizs", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.Quizs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizType = c.Int(nullable: false),
                        QuizTitle = c.String(maxLength: 200),
                        QuizCoverImage = c.String(maxLength: 2000),
                        QuizDescription = c.String(maxLength: 1000),
                        StartButtonText = c.String(maxLength: 100),
                        EnableFacebookShare = c.Boolean(),
                        EnableTwitterShare = c.Boolean(),
                        EnableLinkedinShare = c.Boolean(),
                        AccessibleOfficeId = c.Int(),
                        CompanyAccessibleLevel = c.Int(),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NotificationTemplatesInQuizs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        NotificationTemplateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NotificationTemplates", t => t.NotificationTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.Quizs", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId)
                .Index(t => t.NotificationTemplateId);
            
            CreateTable(
                "dbo.NotificationTemplates",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        OfficeId = c.Int(nullable: false),
                        NotificationType = c.Int(nullable: false),
                        Subject = c.String(),
                        Body = c.String(),
                        SMSText = c.String(),
                        Status = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.QuizBrandingAndStyles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        ImageFileURL = c.String(maxLength: 2000),
                        BackgroundColor = c.String(maxLength: 50),
                        ButtonColor = c.String(maxLength: 50),
                        OptionColor = c.String(maxLength: 50),
                        FontColor = c.String(maxLength: 50),
                        FontType = c.String(maxLength: 200),
                        LastUpdateOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quizs", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.QuizResults",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        Title = c.String(maxLength: 200),
                        Image = c.String(maxLength: 2000),
                        Description = c.String(maxLength: 2000),
                        ActionButtonTitle = c.String(maxLength: 200),
                        ActionButtonURL = c.String(maxLength: 2000),
                        ActionButtonTxtSize = c.String(maxLength: 20),
                        ActionButtonTitleColor = c.String(maxLength: 50),
                        ActionButtonButtonColor = c.String(maxLength: 50),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quizs", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.ResultSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        ShowScoreValue = c.Boolean(),
                        ShowCorrectAnswer = c.Boolean(),
                        MinScore = c.Int(),
                        MaxScore = c.Int(),
                        CustomTxtForAnswerKey = c.String(maxLength: 100),
                        CustomTxtForYourAnswer = c.String(maxLength: 100),
                        CustomTxtForCorrectAnswer = c.String(maxLength: 100),
                        CustomTxtForExplanation = c.String(maxLength: 100),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quizs", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.UserAccessInQuizs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        UserEmail = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quizs", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.QuizBranchingLogics",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AnswerOptionId = c.Int(nullable: false),
                        LinkedQuestionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AnswerOptionsInQuizQuestions", t => t.AnswerOptionId, cascadeDelete: true)
                .Index(t => t.AnswerOptionId);
            
            CreateTable(
                "dbo.RemindersInQuizs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FirstReminder = c.Int(),
                        SecondReminder = c.Int(),
                        ThirdReminder = c.Int(),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizBranchingLogics", "AnswerOptionId", "dbo.AnswerOptionsInQuizQuestions");
            DropForeignKey("dbo.AnswerOptionsInQuizQuestions", "QuestionId", "dbo.QuestionsInQuizs");
            DropForeignKey("dbo.QuestionsInQuizs", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.UserAccessInQuizs", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.ResultSettings", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.QuizResults", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.QuizBrandingAndStyles", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.NotificationTemplatesInQuizs", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.NotificationTemplatesInQuizs", "NotificationTemplateId", "dbo.NotificationTemplates");
            DropIndex("dbo.QuizBranchingLogics", new[] { "AnswerOptionId" });
            DropIndex("dbo.UserAccessInQuizs", new[] { "QuizId" });
            DropIndex("dbo.ResultSettings", new[] { "QuizId" });
            DropIndex("dbo.QuizResults", new[] { "QuizId" });
            DropIndex("dbo.QuizBrandingAndStyles", new[] { "QuizId" });
            DropIndex("dbo.NotificationTemplatesInQuizs", new[] { "NotificationTemplateId" });
            DropIndex("dbo.NotificationTemplatesInQuizs", new[] { "QuizId" });
            DropIndex("dbo.QuestionsInQuizs", new[] { "QuizId" });
            DropIndex("dbo.AnswerOptionsInQuizQuestions", new[] { "QuestionId" });
            DropTable("dbo.RemindersInQuizs");
            DropTable("dbo.QuizBranchingLogics");
            DropTable("dbo.UserAccessInQuizs");
            DropTable("dbo.ResultSettings");
            DropTable("dbo.QuizResults");
            DropTable("dbo.QuizBrandingAndStyles");
            DropTable("dbo.NotificationTemplates");
            DropTable("dbo.NotificationTemplatesInQuizs");
            DropTable("dbo.Quizs");
            DropTable("dbo.QuestionsInQuizs");
            DropTable("dbo.AnswerOptionsInQuizQuestions");
        }
    }
}
