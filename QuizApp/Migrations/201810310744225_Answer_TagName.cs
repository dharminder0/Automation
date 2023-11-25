namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Answer_TagName : DbMigration
    {
        public override void Up()
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
            CreateIndex("dbo.TagsInAnswer", "TagDetailId");
            AddForeignKey("dbo.TagsInAnswer", "TagDetailId", "dbo.TagsDetails", "Id", cascadeDelete: true);
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "CategoryName");
            DropColumn("dbo.TagsInAnswer", "TagId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TagsInAnswer", "TagId", c => c.Int(nullable: false));
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "CategoryName", c => c.String());
            DropForeignKey("dbo.TagsInAnswer", "TagDetailId", "dbo.TagsDetails");
            DropIndex("dbo.TagsInAnswer", new[] { "TagDetailId" });
            DropColumn("dbo.TagsInAnswer", "TagDetailId");
            DropTable("dbo.TagsDetails");
        }
    }
}
