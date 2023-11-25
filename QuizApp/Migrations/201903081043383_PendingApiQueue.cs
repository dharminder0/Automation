namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PendingApiQueue : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PendingApiQueue",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RequestType = c.String(),
                        RequestData = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        CompanyId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Company", t => t.CompanyId)
                .Index(t => t.CompanyId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PendingApiQueue", "CompanyId", "dbo.Company");
            DropIndex("dbo.PendingApiQueue", new[] { "CompanyId" });
            DropTable("dbo.PendingApiQueue");
        }
    }
}
