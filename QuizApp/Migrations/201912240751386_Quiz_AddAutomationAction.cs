namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_AddAutomationAction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActionsInQuiz", "AutomationId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActionsInQuiz", "AutomationId");
        }
    }
}
