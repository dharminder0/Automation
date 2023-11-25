namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ContentsInQuiz : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ContentsInQuiz",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        ContentTitle = c.String(),
                        ContentTitleImage = c.String(),
                        ShowContentTitleImage = c.Boolean(),
                        ContentDescription = c.String(),
                        ContentDescriptionImage = c.String(),
                        ShowContentDescriptionImage = c.Boolean(),
                        AliasTextForNextButton = c.String(),
                        State = c.Int(nullable: false),
                        DisplayOrder = c.Int(nullable: false),
                        Status = c.Int(),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizDetails", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ContentsInQuiz", "QuizId", "dbo.QuizDetails");
            DropIndex("dbo.ContentsInQuiz", new[] { "QuizId" });
            DropTable("dbo.ContentsInQuiz");
        }
    }
}
