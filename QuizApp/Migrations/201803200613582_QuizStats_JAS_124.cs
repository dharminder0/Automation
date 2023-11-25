namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizStats_JAS_124 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizAttempts",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        LeadUserId = c.Int(nullable: false),
                        BusinessUserId = c.Int(),
                        QuizId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        Code = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                        CreatedBy = c.Int(nullable: false),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.QuizQuestionStats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizCode = c.String(),
                        QuestionId = c.Int(nullable: false),
                        AnswerId = c.Int(),
                        StartedOn = c.DateTime(nullable: false),
                        CompletedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.AnswerOptionsInQuizQuestions", t => t.AnswerId)
                .ForeignKey("dbo.QuestionsInQuiz", t => t.QuestionId, cascadeDelete: true)
                .Index(t => t.QuestionId)
                .Index(t => t.AnswerId);
            
            CreateTable(
                "dbo.QuizStats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizCode = c.String(),
                        StartedOn = c.DateTime(nullable: false),
                        CompletedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Quiz", "PublishedCode", c => c.String());
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizQuestionStats", "QuestionId", "dbo.QuestionsInQuiz");
            DropForeignKey("dbo.QuizQuestionStats", "AnswerId", "dbo.AnswerOptionsInQuizQuestions");
            DropForeignKey("dbo.QuizAttempts", "QuizId", "dbo.Quiz");
            DropIndex("dbo.QuizQuestionStats", new[] { "AnswerId" });
            DropIndex("dbo.QuizQuestionStats", new[] { "QuestionId" });
            DropIndex("dbo.QuizAttempts", new[] { "QuizId" });
            DropColumn("dbo.Quiz", "PublishedCode");
            DropTable("dbo.QuizStats");
            DropTable("dbo.QuizQuestionStats");
            DropTable("dbo.QuizAttempts");
        }
    }
}
