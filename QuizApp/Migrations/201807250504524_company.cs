namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class company : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Company",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientCode = c.String(),
                        JobRocketApiUrl = c.String(),
                        JobRocketApiAuthorizationBearer = c.String(),
                        JobRocketClientUrl = c.String(),
                        LeadDashboardApiUrl = c.String(),
                        LeadDashboardApiAuthorizationBearer = c.String(),
                        LeadDashboardClientUrl = c.String(),
                        PrimaryBrandingColor = c.String(),
                        SecondaryBrandingColor = c.String(),
                        LogoUrl = c.String(),
                        CompanyName = c.String(),
                        AlternateClientCodes = c.String(),
                        CompanyWebsiteUrl = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Quiz", "CompanyId", c => c.Int());
            AddColumn("dbo.UserTokens", "CompanyId", c => c.Int(nullable: false));
            CreateIndex("dbo.Quiz", "CompanyId");
            CreateIndex("dbo.UserTokens", "CompanyId");
            AddForeignKey("dbo.UserTokens", "CompanyId", "dbo.Company", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Quiz", "CompanyId", "dbo.Company", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Quiz", "CompanyId", "dbo.Company");
            DropForeignKey("dbo.UserTokens", "CompanyId", "dbo.Company");
            DropIndex("dbo.UserTokens", new[] { "CompanyId" });
            DropIndex("dbo.Quiz", new[] { "CompanyId" });
            DropColumn("dbo.UserTokens", "CompanyId");
            DropColumn("dbo.Quiz", "CompanyId");
            DropTable("dbo.Company");
        }
    }
}
