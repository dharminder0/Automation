namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizTemplatesAndCategory : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Category",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        CategoryId = c.Int(nullable: false),
                        CategoryName = c.String(),
                        ImageUrl = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Quiz", "CategoryId", c => c.Int());
            CreateIndex("dbo.Quiz", "CategoryId");
            AddForeignKey("dbo.Quiz", "CategoryId", "dbo.Category", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Quiz", "CategoryId", "dbo.Category");
            DropIndex("dbo.Quiz", new[] { "CategoryId" });
            DropColumn("dbo.Quiz", "CategoryId");
            DropTable("dbo.Category");
        }
    }
}
