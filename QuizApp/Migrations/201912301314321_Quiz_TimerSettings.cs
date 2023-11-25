namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_TimerSettings : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuiz", "TimerRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "TimeInSecond", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuiz", "TimeInSecond");
            DropColumn("dbo.QuestionsInQuiz", "TimerRequired");
        }
    }
}
