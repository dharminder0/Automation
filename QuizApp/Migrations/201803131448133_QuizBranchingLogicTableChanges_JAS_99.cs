namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBranchingLogicTableChanges_JAS_99 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuizResults", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.QuizBrandingAndStyles", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.ResultSettings", "QuizId", "dbo.Quizs");
            DropForeignKey("dbo.QuestionsInQuizs", "QuizId", "dbo.Quizs");
            CreateTable(
                "dbo.QuizDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentQuizId = c.Int(nullable: false),
                        QuizTitle = c.String(maxLength: 200),
                        QuizCoverImage = c.String(maxLength: 2000),
                        QuizCoverImgXCoordinate = c.Int(),
                        QuizCoverImgYCoordinate = c.Int(),
                        QuizCoverImgHeight = c.Int(),
                        QuizCoverImgWidth = c.Int(),
                        QuizCoverImgAttributionLabel = c.String(),
                        QuizCoverImgAltTag = c.String(),
                        QuizDescription = c.String(maxLength: 1000),
                        StartButtonText = c.String(maxLength: 100),
                        IsBranchingLogicEnabled = c.Boolean(),
                        HideSocialShareButtons = c.Boolean(),
                        EnableFacebookShare = c.Boolean(),
                        EnableTwitterShare = c.Boolean(),
                        EnableLinkedinShare = c.Boolean(),
                        State = c.Int(nullable: false),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quizs", t => t.ParentQuizId, cascadeDelete: true)
                .Index(t => t.ParentQuizId);
            
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "State", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionsInQuizs", "State", c => c.Int(nullable: false));
            AddColumn("dbo.Quizs", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.QuizResults", "State", c => c.Int(nullable: false));
            AddColumn("dbo.QuizBrandingAndStyles", "State", c => c.Int(nullable: false));
            AddColumn("dbo.ResultSettings", "State", c => c.Int(nullable: false));
            AddForeignKey("dbo.QuizResults", "QuizId", "dbo.QuizDetails", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuizBrandingAndStyles", "QuizId", "dbo.QuizDetails", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ResultSettings", "QuizId", "dbo.QuizDetails", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuestionsInQuizs", "QuizId", "dbo.QuizDetails", "Id", cascadeDelete: true);
            DropColumn("dbo.Quizs", "QuizTitle");
            DropColumn("dbo.Quizs", "QuizCoverImage");
            DropColumn("dbo.Quizs", "QuizCoverImgXCoordinate");
            DropColumn("dbo.Quizs", "QuizCoverImgYCoordinate");
            DropColumn("dbo.Quizs", "QuizCoverImgHeight");
            DropColumn("dbo.Quizs", "QuizCoverImgWidth");
            DropColumn("dbo.Quizs", "QuizCoverImgAttributionLabel");
            DropColumn("dbo.Quizs", "QuizCoverImgAltTag");
            DropColumn("dbo.Quizs", "QuizDescription");
            DropColumn("dbo.Quizs", "StartButtonText");
            DropColumn("dbo.Quizs", "IsBranchingLogicEnabled");
            DropColumn("dbo.Quizs", "HideSocialShareButtons");
            DropColumn("dbo.Quizs", "EnableFacebookShare");
            DropColumn("dbo.Quizs", "EnableTwitterShare");
            DropColumn("dbo.Quizs", "EnableLinkedinShare");
            DropColumn("dbo.Quizs", "CreatedOn");
            DropColumn("dbo.Quizs", "CreatedBy");
            DropColumn("dbo.Quizs", "LastUpdatedOn");
            DropColumn("dbo.Quizs", "LastUpdatedBy");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Quizs", "LastUpdatedBy", c => c.Int());
            AddColumn("dbo.Quizs", "LastUpdatedOn", c => c.DateTime());
            AddColumn("dbo.Quizs", "CreatedBy", c => c.Int(nullable: false));
            AddColumn("dbo.Quizs", "CreatedOn", c => c.DateTime(nullable: false));
            AddColumn("dbo.Quizs", "EnableLinkedinShare", c => c.Boolean());
            AddColumn("dbo.Quizs", "EnableTwitterShare", c => c.Boolean());
            AddColumn("dbo.Quizs", "EnableFacebookShare", c => c.Boolean());
            AddColumn("dbo.Quizs", "HideSocialShareButtons", c => c.Boolean());
            AddColumn("dbo.Quizs", "IsBranchingLogicEnabled", c => c.Boolean());
            AddColumn("dbo.Quizs", "StartButtonText", c => c.String(maxLength: 100));
            AddColumn("dbo.Quizs", "QuizDescription", c => c.String(maxLength: 1000));
            AddColumn("dbo.Quizs", "QuizCoverImgAltTag", c => c.String());
            AddColumn("dbo.Quizs", "QuizCoverImgAttributionLabel", c => c.String());
            AddColumn("dbo.Quizs", "QuizCoverImgWidth", c => c.Int());
            AddColumn("dbo.Quizs", "QuizCoverImgHeight", c => c.Int());
            AddColumn("dbo.Quizs", "QuizCoverImgYCoordinate", c => c.Int());
            AddColumn("dbo.Quizs", "QuizCoverImgXCoordinate", c => c.Int());
            AddColumn("dbo.Quizs", "QuizCoverImage", c => c.String(maxLength: 2000));
            AddColumn("dbo.Quizs", "QuizTitle", c => c.String(maxLength: 200));
            DropForeignKey("dbo.QuestionsInQuizs", "QuizId", "dbo.QuizDetails");
            DropForeignKey("dbo.ResultSettings", "QuizId", "dbo.QuizDetails");
            DropForeignKey("dbo.QuizBrandingAndStyles", "QuizId", "dbo.QuizDetails");
            DropForeignKey("dbo.QuizDetails", "ParentQuizId", "dbo.Quizs");
            DropForeignKey("dbo.QuizResults", "QuizId", "dbo.QuizDetails");
            DropIndex("dbo.QuizDetails", new[] { "ParentQuizId" });
            DropColumn("dbo.ResultSettings", "State");
            DropColumn("dbo.QuizBrandingAndStyles", "State");
            DropColumn("dbo.QuizResults", "State");
            DropColumn("dbo.Quizs", "Status");
            DropColumn("dbo.QuestionsInQuizs", "State");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "State");
            DropTable("dbo.QuizDetails");
            AddForeignKey("dbo.QuestionsInQuizs", "QuizId", "dbo.Quizs", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ResultSettings", "QuizId", "dbo.Quizs", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuizBrandingAndStyles", "QuizId", "dbo.Quizs", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuizResults", "QuizId", "dbo.Quizs", "Id", cascadeDelete: true);
        }
    }
}
