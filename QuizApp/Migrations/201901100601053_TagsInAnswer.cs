namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TagsInAnswer : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.TagsInAnswer", "TagDetailId", "dbo.TagsDetails");
            DropIndex("dbo.TagsInAnswer", new[] { "TagDetailId" });
            AddColumn("dbo.TagsInAnswer", "TagId", c => c.Int(nullable: false));
            AddColumn("dbo.TagsInAnswer", "TagCategoryId", c => c.Int(nullable: false));
            DropColumn("dbo.TagsInAnswer", "TagDetailId");
            DropTable("dbo.TagsDetails");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TagsDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TagId = c.Int(nullable: false),
                        TagName = c.String(),
                        CategoryName = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.TagsInAnswer", "TagDetailId", c => c.Int(nullable: false));
            DropColumn("dbo.TagsInAnswer", "TagCategoryId");
            DropColumn("dbo.TagsInAnswer", "TagId");
            CreateIndex("dbo.TagsInAnswer", "TagDetailId");
            AddForeignKey("dbo.TagsInAnswer", "TagDetailId", "dbo.TagsDetails", "Id", cascadeDelete: true);
        }
    }
}
