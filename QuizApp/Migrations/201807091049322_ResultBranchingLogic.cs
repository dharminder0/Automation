namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResultBranchingLogic : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActionsInQuiz", t => t.LinkedActionId)
                .ForeignKey("dbo.ContentsInQuiz", t => t.LinkedContentId)
                .ForeignKey("dbo.QuizResults", t => t.ResultId)
                .Index(t => t.ResultId)
                .Index(t => t.LinkedActionId)
                .Index(t => t.LinkedContentId);
            
            AddColumn("dbo.QuizBranchingLogic", "LinkedContentId", c => c.Int());
            AddColumn("dbo.QuizBranchingLogic", "ContentId", c => c.Int());
            AddColumn("dbo.QuizBranchingLogic", "XCordinate", c => c.Int());
            AddColumn("dbo.QuizBranchingLogic", "YCordinate", c => c.Int());
            AddColumn("dbo.QuizStartSetting", "StartingContentId", c => c.Int());
            CreateIndex("dbo.QuizStartSetting", "StartingContentId");
            CreateIndex("dbo.QuizBranchingLogic", "LinkedContentId");
            CreateIndex("dbo.QuizBranchingLogic", "ContentId");
            AddForeignKey("dbo.QuizStartSetting", "StartingContentId", "dbo.ContentsInQuiz", "Id");
            AddForeignKey("dbo.QuizBranchingLogic", "ContentId", "dbo.ContentsInQuiz", "Id");
            AddForeignKey("dbo.QuizBranchingLogic", "LinkedContentId", "dbo.ContentsInQuiz", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ResultBranchingLogic", "ResultId", "dbo.QuizResults");
            DropForeignKey("dbo.ResultBranchingLogic", "LinkedContentId", "dbo.ContentsInQuiz");
            DropForeignKey("dbo.ResultBranchingLogic", "LinkedActionId", "dbo.ActionsInQuiz");
            DropForeignKey("dbo.QuizBranchingLogic", "LinkedContentId", "dbo.ContentsInQuiz");
            DropForeignKey("dbo.QuizBranchingLogic", "ContentId", "dbo.ContentsInQuiz");
            DropForeignKey("dbo.QuizStartSetting", "StartingContentId", "dbo.ContentsInQuiz");
            DropIndex("dbo.ResultBranchingLogic", new[] { "LinkedContentId" });
            DropIndex("dbo.ResultBranchingLogic", new[] { "LinkedActionId" });
            DropIndex("dbo.ResultBranchingLogic", new[] { "ResultId" });
            DropIndex("dbo.QuizBranchingLogic", new[] { "ContentId" });
            DropIndex("dbo.QuizBranchingLogic", new[] { "LinkedContentId" });
            DropIndex("dbo.QuizStartSetting", new[] { "StartingContentId" });
            DropColumn("dbo.QuizStartSetting", "StartingContentId");
            DropColumn("dbo.QuizBranchingLogic", "YCordinate");
            DropColumn("dbo.QuizBranchingLogic", "XCordinate");
            DropColumn("dbo.QuizBranchingLogic", "ContentId");
            DropColumn("dbo.QuizBranchingLogic", "LinkedContentId");
            DropTable("dbo.ResultBranchingLogic");
        }
    }
}
