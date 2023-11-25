namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UnknownLead_Configuration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ResultIdsInConfigurationDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ResultId = c.Int(nullable: false),
                        ConfigurationDetailsId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConfigurationDetails", t => t.ConfigurationDetailsId)
                .ForeignKey("dbo.QuizResults", t => t.ResultId, cascadeDelete: true)
                .Index(t => t.ResultId)
                .Index(t => t.ConfigurationDetailsId);
            
            AddColumn("dbo.ConfigurationDetails", "SourceType", c => c.String());
            AddColumn("dbo.ConfigurationDetails", "SourceId", c => c.String());
            AddColumn("dbo.ConfigurationDetails", "PrivacyLink", c => c.String());
            AddColumn("dbo.ConfigurationDetails", "Status", c => c.Int(nullable: false));
            AddColumn("dbo.ConfigurationDetails", "ConfigurationType", c => c.String());

            Sql("update ConfigurationDetails set Status = 1, ConfigurationType = 'KNOWN_LEADS'");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ResultIdsInConfigurationDetails", "ResultId", "dbo.QuizResults");
            DropForeignKey("dbo.ResultIdsInConfigurationDetails", "ConfigurationDetailsId", "dbo.ConfigurationDetails");
            DropIndex("dbo.ResultIdsInConfigurationDetails", new[] { "ConfigurationDetailsId" });
            DropIndex("dbo.ResultIdsInConfigurationDetails", new[] { "ResultId" });
            DropColumn("dbo.ConfigurationDetails", "ConfigurationType");
            DropColumn("dbo.ConfigurationDetails", "Status");
            DropColumn("dbo.ConfigurationDetails", "PrivacyLink");
            DropColumn("dbo.ConfigurationDetails", "SourceId");
            DropColumn("dbo.ConfigurationDetails", "SourceType");
            DropTable("dbo.ResultIdsInConfigurationDetails");
        }
    }
}
