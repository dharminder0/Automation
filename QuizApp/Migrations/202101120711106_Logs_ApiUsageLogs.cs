namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Logs_ApiUsageLogs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ApiUsageLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Controller = c.String(),
                        Action = c.String(),
                        Body = c.String(),
                        Url = c.String(),
                        RequestDate = c.DateTime(nullable: false),
                        Response = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ApiUsageLogs");
        }
    }
}
