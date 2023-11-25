namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkPackage_JAS_198 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WorkPackageInfo",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LeadUserId = c.Int(nullable: false),
                        QuizId = c.Int(nullable: false),
                        BusinessUserId = c.Int(),
                        CampaignId = c.Int(),
                        CampaignName = c.String(maxLength: 1000),
                        Status = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            AddColumn("dbo.QuizAttempts", "CampaignName", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.WorkPackageInfo", "QuizId", "dbo.Quiz");
            DropIndex("dbo.WorkPackageInfo", new[] { "QuizId" });
            DropColumn("dbo.QuizAttempts", "CampaignName");
            DropTable("dbo.WorkPackageInfo");
        }
    }
}
