namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_ChangeLeadDataInAction : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LeadDataInAction",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LeadUserId = c.Int(nullable: false),
                        ActionId = c.Int(nullable: false),
                        AppointmentTypeId = c.Int(),
                        ReportEmails = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ActionsInQuiz", t => t.ActionId, cascadeDelete: true)
                .Index(t => t.ActionId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LeadDataInAction", "ActionId", "dbo.ActionsInQuiz");
            DropIndex("dbo.LeadDataInAction", new[] { "ActionId" });
            DropTable("dbo.LeadDataInAction");
        }
    }
}
