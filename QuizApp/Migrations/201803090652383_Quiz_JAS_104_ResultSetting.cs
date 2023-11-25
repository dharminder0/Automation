namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_JAS_104_ResultSetting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.NotificationTemplatesInResults",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResultId = c.Int(nullable: false),
                        NotificationTemplateId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NotificationTemplates", t => t.NotificationTemplateId, cascadeDelete: true)
                .ForeignKey("dbo.QuizResults", t => t.ResultId, cascadeDelete: true)
                .Index(t => t.ResultId)
                .Index(t => t.NotificationTemplateId);
            
            AddColumn("dbo.QuizResults", "ShowResultImage", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizResults", "OpenLinkInNewTab", c => c.Boolean());
            AddColumn("dbo.QuizResults", "ActionButtonColor", c => c.String(maxLength: 50));
            AddColumn("dbo.QuizResults", "Status", c => c.Int());
            AddColumn("dbo.ResultSettings", "CustomTxtForScoreValueInResult", c => c.String());
            DropColumn("dbo.QuizResults", "ActionButtonTitle");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizResults", "ActionButtonTitle", c => c.String(maxLength: 200));
            DropForeignKey("dbo.NotificationTemplatesInResults", "ResultId", "dbo.QuizResults");
            DropForeignKey("dbo.NotificationTemplatesInResults", "NotificationTemplateId", "dbo.NotificationTemplates");
            DropIndex("dbo.NotificationTemplatesInResults", new[] { "NotificationTemplateId" });
            DropIndex("dbo.NotificationTemplatesInResults", new[] { "ResultId" });
            DropColumn("dbo.ResultSettings", "CustomTxtForScoreValueInResult");
            DropColumn("dbo.QuizResults", "Status");
            DropColumn("dbo.QuizResults", "ActionButtonColor");
            DropColumn("dbo.QuizResults", "OpenLinkInNewTab");
            DropColumn("dbo.QuizResults", "ShowResultImage");
            DropTable("dbo.NotificationTemplatesInResults");
        }
    }
}
