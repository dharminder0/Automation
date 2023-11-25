namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigurationDetails_TemplateControls : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfigurationDetails", "SendEmail", c => c.Boolean(nullable: true));
            AddColumn("dbo.ConfigurationDetails", "SendSms", c => c.Boolean(nullable: true));
            AddColumn("dbo.ConfigurationDetails", "SendWhatsApp", c => c.Boolean(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfigurationDetails", "SendWhatsApp");
            DropColumn("dbo.ConfigurationDetails", "SendSms");
            DropColumn("dbo.ConfigurationDetails", "SendEmail");
        }
    }
}
