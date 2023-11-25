namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class brachinglogic_QuizId : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.BranchingLogic", "QuizId", "dbo.Quiz");
            AddForeignKey("dbo.BranchingLogic", "QuizId", "dbo.QuizDetails", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BranchingLogic", "QuizId", "dbo.QuizDetails");
            AddForeignKey("dbo.BranchingLogic", "QuizId", "dbo.Quiz", "Id", cascadeDelete: true);
        }
    }
}
