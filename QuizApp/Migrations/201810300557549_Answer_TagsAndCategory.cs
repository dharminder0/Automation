namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Answer_TagsAndCategory : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuizQuestionStats", "AnswerId", "dbo.AnswerOptionsInQuizQuestions");
            DropIndex("dbo.QuizQuestionStats", new[] { "AnswerId" });
            CreateTable(
                "dbo.QuizAnswerStats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizQuestionStatsId = c.Int(nullable: false),
                        AnswerId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AnswerOptionsInQuizQuestions", t => t.AnswerId)
                .ForeignKey("dbo.QuizQuestionStats", t => t.QuizQuestionStatsId, cascadeDelete: true)
                .Index(t => t.QuizQuestionStatsId)
                .Index(t => t.AnswerId);
            
            CreateTable(
                "dbo.TagsInAnswer",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AnswerOptionsId = c.Int(nullable: false),
                        TagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AnswerOptionsInQuizQuestions", t => t.AnswerOptionsId, cascadeDelete: true)
                .Index(t => t.AnswerOptionsId);

            Sql("Insert into QuizAnswerStats(QuizQuestionStatsId,AnswerId) select Id, AnswerId from QuizQuestionStats where AnswerId is not null");

            AddColumn("dbo.AnswerOptionsInQuizQuestions", "CategoryName", c => c.String());
            DropColumn("dbo.QuizQuestionStats", "AnswerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizQuestionStats", "AnswerId", c => c.Int());
            DropForeignKey("dbo.TagsInAnswer", "AnswerOptionsId", "dbo.AnswerOptionsInQuizQuestions");
            DropForeignKey("dbo.QuizAnswerStats", "QuizQuestionStatsId", "dbo.QuizQuestionStats");
            DropForeignKey("dbo.QuizAnswerStats", "AnswerId", "dbo.AnswerOptionsInQuizQuestions");
            DropIndex("dbo.TagsInAnswer", new[] { "AnswerOptionsId" });
            DropIndex("dbo.QuizAnswerStats", new[] { "AnswerId" });
            DropIndex("dbo.QuizAnswerStats", new[] { "QuizQuestionStatsId" });
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "CategoryName");
            DropTable("dbo.TagsInAnswer");
            DropTable("dbo.QuizAnswerStats");
            CreateIndex("dbo.QuizQuestionStats", "AnswerId");
            AddForeignKey("dbo.QuizQuestionStats", "AnswerId", "dbo.AnswerOptionsInQuizQuestions", "Id");
        }
    }
}
