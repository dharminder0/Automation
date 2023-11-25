namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AttachmentsInQuiz : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttachmentsInQuiz",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        Title = c.String(),
                        Description = c.String(),
                        Status = c.Int(),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.AttachmentsInQuiz", "QuizId", "dbo.Quiz");
            DropIndex("dbo.AttachmentsInQuiz", new[] { "QuizId" });
            DropTable("dbo.AttachmentsInQuiz");
        }
    }
}
