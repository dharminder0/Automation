namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_ResultEmailSMS : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ResultQueues", "QuizAttemptId", "dbo.QuizAttempts");
            DropForeignKey("dbo.NotificationTemplatesInResult", "NotificationTemplateId", "dbo.NotificationTemplate");
            DropForeignKey("dbo.NotificationTemplatesInResult", "ResultId", "dbo.QuizResults");
            DropIndex("dbo.NotificationTemplatesInResult", new[] { "ResultId" });
            DropIndex("dbo.NotificationTemplatesInResult", new[] { "NotificationTemplateId" });
            DropIndex("dbo.ResultQueues", new[] { "QuizAttemptId" });
            DropColumn("dbo.ResultSettings", "RevealAfter");
            DropTable("dbo.NotificationTemplatesInResult");
            DropTable("dbo.ResultQueues");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.ResultQueues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizAttemptId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        QueuedOn = c.DateTime(nullable: false),
                        SentOn = c.DateTime(),
                        Sent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.NotificationTemplatesInResult",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResultId = c.Int(nullable: false),
                        NotificationTemplateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.ResultSettings", "RevealAfter", c => c.Long());
            CreateIndex("dbo.ResultQueues", "QuizAttemptId");
            CreateIndex("dbo.NotificationTemplatesInResult", "NotificationTemplateId");
            CreateIndex("dbo.NotificationTemplatesInResult", "ResultId");
            AddForeignKey("dbo.NotificationTemplatesInResult", "ResultId", "dbo.QuizResults", "Id", cascadeDelete: true);
            AddForeignKey("dbo.NotificationTemplatesInResult", "NotificationTemplateId", "dbo.NotificationTemplate", "Id", cascadeDelete: true);
            AddForeignKey("dbo.ResultQueues", "QuizAttemptId", "dbo.QuizAttempts", "Id", cascadeDelete: true);
        }
    }
}
