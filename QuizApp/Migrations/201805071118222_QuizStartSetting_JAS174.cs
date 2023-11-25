namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizStartSetting_JAS174 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizStartSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StartingQuestionId = c.Int(),
                        StartingResultId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuestionsInQuiz", t => t.StartingQuestionId)
                .ForeignKey("dbo.QuizResults", t => t.StartingResultId)
                .Index(t => t.StartingQuestionId)
                .Index(t => t.StartingResultId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizStartSetting", "StartingResultId", "dbo.QuizResults");
            DropForeignKey("dbo.QuizStartSetting", "StartingQuestionId", "dbo.QuestionsInQuiz");
            DropIndex("dbo.QuizStartSetting", new[] { "StartingResultId" });
            DropIndex("dbo.QuizStartSetting", new[] { "StartingQuestionId" });
            DropTable("dbo.QuizStartSetting");
        }
    }
}
