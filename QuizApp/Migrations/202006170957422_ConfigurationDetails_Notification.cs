namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigurationDetails_Notification : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfigurationDetails", "IsUpdatedSend", c => c.Boolean(nullable: false));
            AddColumn("dbo.ConfigurationDetails", "Subject", c => c.String());
            AddColumn("dbo.ConfigurationDetails", "Body", c => c.String());
            AddColumn("dbo.ConfigurationDetails", "SMSText", c => c.String());
            AddColumn("dbo.ConfigurationDetails", "SendMailNotRequired", c => c.Boolean(nullable: false));
            AddColumn("dbo.ConfigurationDetails", "CompanyCode", c => c.String());
            AddColumn("dbo.ConfigurationDetails", "CreatedOn", c => c.DateTime());
            AddColumn("dbo.ConfigurationDetails", "UpdatedOn", c => c.DateTime());
            DropColumn("dbo.WorkPackageInfo", "UpdatedOn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.WorkPackageInfo", "UpdatedOn", c => c.DateTime());
            DropColumn("dbo.ConfigurationDetails", "UpdatedOn");
            DropColumn("dbo.ConfigurationDetails", "CreatedOn");
            DropColumn("dbo.ConfigurationDetails", "CompanyCode");
            DropColumn("dbo.ConfigurationDetails", "SendMailNotRequired");
            DropColumn("dbo.ConfigurationDetails", "SMSText");
            DropColumn("dbo.ConfigurationDetails", "Body");
            DropColumn("dbo.ConfigurationDetails", "Subject");
            DropColumn("dbo.ConfigurationDetails", "IsUpdatedSend");
        }
    }
}
