namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LinkedDataInAction_IsUpdated : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.LeadDataInAction", "IsUpdatedSend", c => c.Boolean(nullable: false));
            AddColumn("dbo.LeadDataInAction", "Subject", c => c.String());
            AddColumn("dbo.LeadDataInAction", "Body", c => c.String());
            AddColumn("dbo.LeadDataInAction", "SMSText", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.LeadDataInAction", "SMSText");
            DropColumn("dbo.LeadDataInAction", "Body");
            DropColumn("dbo.LeadDataInAction", "Subject");
            DropColumn("dbo.LeadDataInAction", "IsUpdatedSend");
        }
    }
}
