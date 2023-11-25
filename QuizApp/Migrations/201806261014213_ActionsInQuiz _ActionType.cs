namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionsInQuiz_ActionType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActionsInQuiz", "ActionType", c => c.Int(nullable: false));
            DropColumn("dbo.ActionsInQuiz", "IsAppointment");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ActionsInQuiz", "IsAppointment", c => c.Boolean());
            DropColumn("dbo.ActionsInQuiz", "ActionType");
        }
    }
}
