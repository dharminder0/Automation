namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReminderQueues_JAS_174 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ReminderQueues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ReminderLevel = c.Int(nullable: false),
                        QuizCode = c.String(),
                        ReminderInQuizId = c.Int(),
                        Type = c.Int(nullable: false),
                        QueuedOn = c.DateTime(nullable: false),
                        SentOn = c.DateTime(),
                        Sent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RemindersInQuiz", t => t.ReminderInQuizId)
                .Index(t => t.ReminderInQuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ReminderQueues", "ReminderInQuizId", "dbo.RemindersInQuiz");
            DropIndex("dbo.ReminderQueues", new[] { "ReminderInQuizId" });
            DropTable("dbo.ReminderQueues");
        }
    }
}
