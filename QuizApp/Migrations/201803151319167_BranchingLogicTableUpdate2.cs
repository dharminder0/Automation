namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BranchingLogicTableUpdate2 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.QuizBranchingLogic", "IsDisabled");
            DropColumn("dbo.QuizBranchingLogic", "State");
            DropColumn("dbo.QuizBranchingLogic", "LastUpdatedBy");
            DropColumn("dbo.QuizBranchingLogic", "LastUpdatedOn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizBranchingLogic", "LastUpdatedOn", c => c.DateTime());
            AddColumn("dbo.QuizBranchingLogic", "LastUpdatedBy", c => c.Int());
            AddColumn("dbo.QuizBranchingLogic", "State", c => c.Int(nullable: false));
            AddColumn("dbo.QuizBranchingLogic", "IsDisabled", c => c.Boolean(nullable: false));
        }
    }
}
