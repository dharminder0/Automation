namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_AnswerTypeAndRevealResult : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ResultQueues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizAttemptId = c.Int(nullable: false),
                        Type = c.Int(nullable: false),
                        QueuedOn = c.DateTime(nullable: false),
                        SentOn = c.DateTime(),
                        Sent = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizAttempts", t => t.QuizAttemptId, cascadeDelete: true)
                .Index(t => t.QuizAttemptId);
            
            AddColumn("dbo.QuestionsInQuiz", "NextButtonText", c => c.String(maxLength: 50));
            AddColumn("dbo.QuestionsInQuiz", "NextButtonTxtSize", c => c.String(maxLength: 50));
            AddColumn("dbo.QuestionsInQuiz", "NextButtonTxtColor", c => c.String(maxLength: 50));
            AddColumn("dbo.QuestionsInQuiz", "NextButtonColor", c => c.String(maxLength: 50));
            AddColumn("dbo.QuestionsInQuiz", "AnswerType", c => c.Int(nullable: false));
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "IsCorrectForMultipleAnswer", c => c.Boolean());
            AddColumn("dbo.QuizAnswerStats", "AnswerText", c => c.String());
            AddColumn("dbo.ResultSettings", "RevealAfter", c => c.Long());
            Sql("update QuestionsInQuiz set AnswerType = 1 , NextButtonColor = '#62afe0' , NextButtonText = 'Next', NextButtonTxtColor = '#fff', NextButtonTxtSize = '24px'");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ResultQueues", "QuizAttemptId", "dbo.QuizAttempts");
            DropIndex("dbo.ResultQueues", new[] { "QuizAttemptId" });
            DropColumn("dbo.ResultSettings", "RevealAfter");
            DropColumn("dbo.QuizAnswerStats", "AnswerText");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "IsCorrectForMultipleAnswer");
            DropColumn("dbo.QuestionsInQuiz", "AnswerType");
            DropColumn("dbo.QuestionsInQuiz", "NextButtonColor");
            DropColumn("dbo.QuestionsInQuiz", "NextButtonTxtColor");
            DropColumn("dbo.QuestionsInQuiz", "NextButtonTxtSize");
            DropColumn("dbo.QuestionsInQuiz", "NextButtonText");
            DropTable("dbo.ResultQueues");
        }
    }
}
