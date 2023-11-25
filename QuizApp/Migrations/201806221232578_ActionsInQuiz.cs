namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ActionsInQuiz : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ActionsInQuiz",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        Title = c.String(),
                        AppointmentId = c.Int(),
                        ReportEmails = c.String(),
                        State = c.Int(nullable: false),
                        Status = c.Int(),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizDetails", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ActionsInQuiz", "QuizId", "dbo.QuizDetails");
            DropIndex("dbo.ActionsInQuiz", new[] { "QuizId" });
            DropTable("dbo.ActionsInQuiz");
        }
    }
}
