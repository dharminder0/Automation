namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Test : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttachmentsInConfiguration",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FileName = c.String(),
                        FileIdentifier = c.String(),
                        FileLink = c.String(),
                        ConfigurationDetailsId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConfigurationDetails", t => t.ConfigurationDetailsId)
                .Index(t => t.ConfigurationDetailsId);
            
            AddColumn("dbo.ConfigurationDetails", "Test", c => c.String());
            AddColumn("dbo.AttachmentsInNotificationTemplate", "FileIdentifier", c => c.String());
            AddColumn("dbo.QuizBrandingAndStyle", "LogoPublicId", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttachmentsInConfiguration", "ConfigurationDetailsId", "dbo.ConfigurationDetails");
            DropIndex("dbo.AttachmentsInConfiguration", new[] { "ConfigurationDetailsId" });
            DropColumn("dbo.QuizBrandingAndStyle", "LogoPublicId");
            DropColumn("dbo.AttachmentsInNotificationTemplate", "FileIdentifier");
            DropColumn("dbo.ConfigurationDetails", "Test");
            DropTable("dbo.AttachmentsInConfiguration");
        }
    }
}
