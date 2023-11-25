namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TemplateAttachment : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttachmentsInNotificationTemplate",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        NotificationTemplateId = c.Int(nullable: false),
                        Title = c.String(),
                        Description = c.String(),
                        LastUpdatedOn = c.DateTime(nullable: false),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.NotificationTemplate", t => t.NotificationTemplateId, cascadeDelete: true)
                .Index(t => t.NotificationTemplateId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttachmentsInNotificationTemplate", "NotificationTemplateId", "dbo.NotificationTemplate");
            DropIndex("dbo.AttachmentsInNotificationTemplate", new[] { "NotificationTemplateId" });
            DropTable("dbo.AttachmentsInNotificationTemplate");
        }
    }
}
