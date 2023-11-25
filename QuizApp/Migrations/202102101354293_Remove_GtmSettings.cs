namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Remove_GtmSettings : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.GtmSettings", "CompanyId", "dbo.Company");
            DropIndex("dbo.GtmSettings", new[] { "CompanyId" });
            DropTable("dbo.GtmSettings");
        }
        
        public override void Down()
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
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.GtmSettings", "CompanyId");
            AddForeignKey("dbo.GtmSettings", "CompanyId", "dbo.Company", "Id", cascadeDelete: true);
        }
    }
}
