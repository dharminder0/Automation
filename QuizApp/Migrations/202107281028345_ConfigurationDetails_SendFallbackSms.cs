namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigurationDetails_SendFallbackSms : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfigurationDetails", "SendFallbackSms", c => c.Boolean(nullable: true));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfigurationDetails", "SendFallbackSms");
        }
    }
}
