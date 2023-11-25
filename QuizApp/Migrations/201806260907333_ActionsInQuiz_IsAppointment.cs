namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionsInQuiz_IsAppointment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ActionsInQuiz", "IsAppointment", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ActionsInQuiz", "IsAppointment");
        }
    }
}
