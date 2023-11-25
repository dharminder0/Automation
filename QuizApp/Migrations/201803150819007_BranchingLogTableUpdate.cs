namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BranchingLogTableUpdate : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBranchingLogic", "LinkedResultId", c => c.Int());
            AddColumn("dbo.QuizBranchingLogic", "IsDisabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizBranchingLogic", "State", c => c.Int(nullable: false));
            AddColumn("dbo.QuizBranchingLogic", "LastUpdatedBy", c => c.Int());
            AddColumn("dbo.QuizBranchingLogic", "LastUpdatedOn", c => c.DateTime());
            AlterColumn("dbo.QuizBranchingLogic", "LinkedQuestionId", c => c.Int());
            CreateIndex("dbo.QuizBranchingLogic", "LinkedResultId");
            CreateIndex("dbo.QuizBranchingLogic", "LinkedQuestionId");
            AddForeignKey("dbo.QuizBranchingLogic", "LinkedQuestionId", "dbo.QuestionsInQuiz", "Id");
            AddForeignKey("dbo.QuizBranchingLogic", "LinkedResultId", "dbo.QuizResults", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizBranchingLogic", "LinkedResultId", "dbo.QuizResults");
            DropForeignKey("dbo.QuizBranchingLogic", "LinkedQuestionId", "dbo.QuestionsInQuiz");
            DropIndex("dbo.QuizBranchingLogic", new[] { "LinkedQuestionId" });
            DropIndex("dbo.QuizBranchingLogic", new[] { "LinkedResultId" });
            AlterColumn("dbo.QuizBranchingLogic", "LinkedQuestionId", c => c.Int(nullable: false));
            DropColumn("dbo.QuizBranchingLogic", "LastUpdatedOn");
            DropColumn("dbo.QuizBranchingLogic", "LastUpdatedBy");
            DropColumn("dbo.QuizBranchingLogic", "State");
            DropColumn("dbo.QuizBranchingLogic", "IsDisabled");
            DropColumn("dbo.QuizBranchingLogic", "LinkedResultId");
        }
    }
}
