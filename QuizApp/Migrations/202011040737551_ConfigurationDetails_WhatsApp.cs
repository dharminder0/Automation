namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigurationDetails_WhatsApp : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TemplateParameterInConfigurationDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParaName = c.String(),
                        Position = c.Int(nullable: false),
                        Value = c.String(),
                        ConfigurationDetailsId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConfigurationDetails", t => t.ConfigurationDetailsId, cascadeDelete: true)
                .Index(t => t.ConfigurationDetailsId);
            
            AddColumn("dbo.ConfigurationDetails", "HsmTemplateId", c => c.Int(nullable: false));
            AddColumn("dbo.ConfigurationDetails", "FollowUpMessage", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TemplateParameterInConfigurationDetails", "ConfigurationDetailsId", "dbo.ConfigurationDetails");
            DropIndex("dbo.TemplateParameterInConfigurationDetails", new[] { "ConfigurationDetailsId" });
            DropColumn("dbo.ConfigurationDetails", "FollowUpMessage");
            DropColumn("dbo.ConfigurationDetails", "HsmTemplateId");
            DropTable("dbo.TemplateParameterInConfigurationDetails");
        }
    }
}
