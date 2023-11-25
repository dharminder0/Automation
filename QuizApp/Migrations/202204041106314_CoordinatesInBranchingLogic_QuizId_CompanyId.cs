namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CoordinatesInBranchingLogic_QuizId_CompanyId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.CoordinatesInBranchingLogic", "QuizId", c => c.Int());
            AddColumn("dbo.CoordinatesInBranchingLogic", "CompanyId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.CoordinatesInBranchingLogic", "CompanyId");
            DropColumn("dbo.CoordinatesInBranchingLogic", "QuizId");
        }
    }
}
