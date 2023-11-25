namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkPackageInfo_SentOn : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPackageInfo", "EmailSentOn", c => c.DateTime());
            AddColumn("dbo.WorkPackageInfo", "SMSSentOn", c => c.DateTime());
            AddColumn("dbo.WorkPackageInfo", "WhatsappSentOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPackageInfo", "WhatsappSentOn");
            DropColumn("dbo.WorkPackageInfo", "SMSSentOn");
            DropColumn("dbo.WorkPackageInfo", "EmailSentOn");
        }
    }
}
