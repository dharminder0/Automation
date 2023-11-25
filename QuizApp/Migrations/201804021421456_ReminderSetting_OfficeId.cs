namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReminderSetting_OfficeId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RemindersInQuiz", "OfficeId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.RemindersInQuiz", "OfficeId");
        }
    }
}
