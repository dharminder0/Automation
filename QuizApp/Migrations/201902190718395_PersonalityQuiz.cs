namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PersonalityQuiz : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.PersonalityResultSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        Title = c.String(),
                        Status = c.Int(nullable: false),
                        MaxResult = c.Int(nullable: false),
                        GraphColor = c.String(),
                        ButtonColor = c.String(),
                        ButtonFontColor = c.String(),
                        SideButtonText = c.String(),
                        IsFullWidthEnable = c.Boolean(nullable: false),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizDetails", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            CreateTable(
                "dbo.PersonalityAnswerResultMapping",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        AnswerId = c.Int(nullable: false),
                        ResultId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizResults", t => t.ResultId)
                .ForeignKey("dbo.AnswerOptionsInQuizQuestions", t => t.AnswerId)
                .Index(t => t.AnswerId)
                .Index(t => t.ResultId);
            
            AddColumn("dbo.QuizResults", "DisplayOrder", c => c.Int(nullable: false));
            AddColumn("dbo.QuizResults", "IsPersonalityCorrelatedResult", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PersonalityAnswerResultMapping", "AnswerId", "dbo.AnswerOptionsInQuizQuestions");
            DropForeignKey("dbo.PersonalityAnswerResultMapping", "ResultId", "dbo.QuizResults");
            DropForeignKey("dbo.PersonalityResultSetting", "QuizId", "dbo.QuizDetails");
            DropIndex("dbo.PersonalityAnswerResultMapping", new[] { "ResultId" });
            DropIndex("dbo.PersonalityAnswerResultMapping", new[] { "AnswerId" });
            DropIndex("dbo.PersonalityResultSetting", new[] { "QuizId" });
            DropColumn("dbo.QuizResults", "IsPersonalityCorrelatedResult");
            DropColumn("dbo.QuizResults", "DisplayOrder");
            DropTable("dbo.PersonalityAnswerResultMapping");
            DropTable("dbo.PersonalityResultSetting");
        }
    }
}
