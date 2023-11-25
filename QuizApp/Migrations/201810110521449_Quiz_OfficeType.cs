namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_OfficeType : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.NotificationTemplate", "OfficeId", c => c.String());
            AlterColumn("dbo.Quiz", "AccessibleOfficeId", c => c.String());
            AlterColumn("dbo.RemindersInQuiz", "OfficeId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.RemindersInQuiz", "OfficeId", c => c.Int());
            AlterColumn("dbo.Quiz", "AccessibleOfficeId", c => c.Int());
            AlterColumn("dbo.NotificationTemplate", "OfficeId", c => c.Int());
        }
    }
}
