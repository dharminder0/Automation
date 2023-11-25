namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_CalendarDataInAction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LeadCalendarDataInAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LeadDataInActionId = c.Int(nullable: false),
                        CalendarId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.LeadDataInAction", t => t.LeadDataInActionId, cascadeDelete: true)
                .Index(t => t.LeadDataInActionId);
            
            CreateTable(
                "dbo.LinkedCalendarInAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ActionsInQuizId = c.Int(nullable: false),
                        CalendarId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActionsInQuiz", t => t.ActionsInQuizId, cascadeDelete: true)
                .Index(t => t.ActionsInQuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LinkedCalendarInAction", "ActionsInQuizId", "dbo.ActionsInQuiz");
            DropForeignKey("dbo.LeadCalendarDataInAction", "LeadDataInActionId", "dbo.LeadDataInAction");
            DropIndex("dbo.LinkedCalendarInAction", new[] { "ActionsInQuizId" });
            DropIndex("dbo.LeadCalendarDataInAction", new[] { "LeadDataInActionId" });
            DropTable("dbo.LinkedCalendarInAction");
            DropTable("dbo.LeadCalendarDataInAction");
        }
    }
}
