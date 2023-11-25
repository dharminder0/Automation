namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class GtmSettings : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.GtmSettings",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GtmCode = c.String(),
                        Brand = c.String(),
                        CompanyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Company", t => t.CompanyId, cascadeDelete: true)
                .Index(t => t.CompanyId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.GtmSettings", "CompanyId", "dbo.Company");
            DropIndex("dbo.GtmSettings", new[] { "CompanyId" });
            DropTable("dbo.GtmSettings");
        }
    }
}
