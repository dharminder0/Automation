namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkPackageInfo_Type_CampaignId : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.WorkPackageInfo", "CampaignId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.WorkPackageInfo", "CampaignId", c => c.Int());
        }
    }
}
