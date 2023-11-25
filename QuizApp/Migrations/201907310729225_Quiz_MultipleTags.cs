namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_MultipleTags : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizTagDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TagColor = c.String(),
                        LabelText = c.String(),
                        QuizId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            DropColumn("dbo.Quiz", "TagColor");
            DropColumn("dbo.Quiz", "LabelText");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Quiz", "LabelText", c => c.String());
            AddColumn("dbo.Quiz", "TagColor", c => c.String());
            DropForeignKey("dbo.QuizTagDetails", "QuizId", "dbo.Quiz");
            DropIndex("dbo.QuizTagDetails", new[] { "QuizId" });
            DropTable("dbo.QuizTagDetails");
        }
    }
}
