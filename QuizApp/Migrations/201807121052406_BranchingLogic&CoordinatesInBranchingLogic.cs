namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BranchingLogicCoordinatesInBranchingLogic : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuizStartSetting", "StartingContentId", "dbo.ContentsInQuiz");
            DropForeignKey("dbo.QuizBranchingLogic", "AnswerOptionId", "dbo.AnswerOptionsInQuizQuestions");
            DropForeignKey("dbo.QuizBranchingLogic", "ContentId", "dbo.ContentsInQuiz");
            DropForeignKey("dbo.QuizBranchingLogic", "LinkedContentId", "dbo.ContentsInQuiz");
            DropForeignKey("dbo.QuizBranchingLogic", "LinkedQuestionId", "dbo.QuestionsInQuiz");
            DropForeignKey("dbo.ResultBranchingLogic", "LinkedActionId", "dbo.ActionsInQuiz");
            DropForeignKey("dbo.ResultBranchingLogic", "LinkedContentId", "dbo.ContentsInQuiz");
            DropForeignKey("dbo.ResultBranchingLogic", "ResultId", "dbo.QuizResults");
            DropForeignKey("dbo.QuizBranchingLogic", "LinkedResultId", "dbo.QuizResults");
            DropForeignKey("dbo.QuizStartSetting", "StartingQuestionId", "dbo.QuestionsInQuiz");
            DropForeignKey("dbo.QuizStartSetting", "StartingResultId", "dbo.QuizResults");
            DropIndex("dbo.QuizStartSetting", new[] { "StartingQuestionId" });
            DropIndex("dbo.QuizStartSetting", new[] { "StartingResultId" });
            DropIndex("dbo.QuizStartSetting", new[] { "StartingContentId" });
            DropIndex("dbo.QuizBranchingLogic", new[] { "AnswerOptionId" });
            DropIndex("dbo.QuizBranchingLogic", new[] { "LinkedResultId" });
            DropIndex("dbo.QuizBranchingLogic", new[] { "LinkedQuestionId" });
            DropIndex("dbo.QuizBranchingLogic", new[] { "LinkedContentId" });
            DropIndex("dbo.QuizBranchingLogic", new[] { "ContentId" });
            DropIndex("dbo.ResultBranchingLogic", new[] { "ResultId" });
            DropIndex("dbo.ResultBranchingLogic", new[] { "LinkedActionId" });
            DropIndex("dbo.ResultBranchingLogic", new[] { "LinkedContentId" });
            CreateTable(
                "dbo.BranchingLogic",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        SourceTypeId = c.Int(nullable: false),
                        SourceObjectId = c.Int(nullable: false),
                        DestinationTypeId = c.Int(),
                        DestinationObjectId = c.Int(),
                        IsStartingPoint = c.Boolean(nullable: false),
                        IsEndPoint = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.CoordinatesInBranchingLogic",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ObjectTypeId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        XCoordinate = c.String(),
                        YCoordinate = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropTable("dbo.QuizStartSetting");
            DropTable("dbo.QuizBranchingLogic");
            DropTable("dbo.ResultBranchingLogic");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ResultBranchingLogic",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResultId = c.Int(),
                        LinkedActionId = c.Int(),
                        LinkedContentId = c.Int(),
                        XCordinate = c.Int(),
                        YCordinate = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.QuizBranchingLogic",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AnswerOptionId = c.Int(nullable: false),
                        LinkedResultId = c.Int(),
                        LinkedQuestionId = c.Int(),
                        LinkedContentId = c.Int(),
                        ContentId = c.Int(),
                        XCordinate = c.Int(),
                        YCordinate = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.QuizStartSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartingQuestionId = c.Int(),
                        StartingResultId = c.Int(),
                        StartingContentId = c.Int(),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.BranchingLogic", "QuizId", "dbo.Quiz");
            DropIndex("dbo.BranchingLogic", new[] { "QuizId" });
            DropTable("dbo.CoordinatesInBranchingLogic");
            DropTable("dbo.BranchingLogic");
            CreateIndex("dbo.ResultBranchingLogic", "LinkedContentId");
            CreateIndex("dbo.ResultBranchingLogic", "LinkedActionId");
            CreateIndex("dbo.ResultBranchingLogic", "ResultId");
            CreateIndex("dbo.QuizBranchingLogic", "ContentId");
            CreateIndex("dbo.QuizBranchingLogic", "LinkedContentId");
            CreateIndex("dbo.QuizBranchingLogic", "LinkedQuestionId");
            CreateIndex("dbo.QuizBranchingLogic", "LinkedResultId");
            CreateIndex("dbo.QuizBranchingLogic", "AnswerOptionId");
            CreateIndex("dbo.QuizStartSetting", "StartingContentId");
            CreateIndex("dbo.QuizStartSetting", "StartingResultId");
            CreateIndex("dbo.QuizStartSetting", "StartingQuestionId");
            AddForeignKey("dbo.QuizStartSetting", "StartingResultId", "dbo.QuizResults", "Id");
            AddForeignKey("dbo.QuizStartSetting", "StartingQuestionId", "dbo.QuestionsInQuiz", "Id");
            AddForeignKey("dbo.QuizBranchingLogic", "LinkedResultId", "dbo.QuizResults", "Id");
            AddForeignKey("dbo.ResultBranchingLogic", "ResultId", "dbo.QuizResults", "Id");
            AddForeignKey("dbo.ResultBranchingLogic", "LinkedContentId", "dbo.ContentsInQuiz", "Id");
            AddForeignKey("dbo.ResultBranchingLogic", "LinkedActionId", "dbo.ActionsInQuiz", "Id");
            AddForeignKey("dbo.QuizBranchingLogic", "LinkedQuestionId", "dbo.QuestionsInQuiz", "Id");
            AddForeignKey("dbo.QuizBranchingLogic", "LinkedContentId", "dbo.ContentsInQuiz", "Id");
            AddForeignKey("dbo.QuizBranchingLogic", "ContentId", "dbo.ContentsInQuiz", "Id");
            AddForeignKey("dbo.QuizBranchingLogic", "AnswerOptionId", "dbo.AnswerOptionsInQuizQuestions", "Id", cascadeDelete: true);
            AddForeignKey("dbo.QuizStartSetting", "StartingContentId", "dbo.ContentsInQuiz", "Id");
        }
    }
}
