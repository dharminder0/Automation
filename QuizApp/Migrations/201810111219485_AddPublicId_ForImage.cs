namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPublicId_ForImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "PublicId", c => c.String());
            AddColumn("dbo.BadgesInQuiz", "PublicId", c => c.String());
            AddColumn("dbo.ContentsInQuiz", "PublicIdForContentTitle", c => c.String());
            AddColumn("dbo.ContentsInQuiz", "PublicIdForContentDescription", c => c.String());
            AddColumn("dbo.QuestionsInQuiz", "PublicId", c => c.String());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "PublicId", c => c.String());
            AddColumn("dbo.QuizResults", "PublicId", c => c.String());
            AddColumn("dbo.QuizBrandingAndStyle", "PublicId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "PublicId");
            DropColumn("dbo.QuizResults", "PublicId");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "PublicId");
            DropColumn("dbo.QuestionsInQuiz", "PublicId");
            DropColumn("dbo.ContentsInQuiz", "PublicIdForContentDescription");
            DropColumn("dbo.ContentsInQuiz", "PublicIdForContentTitle");
            DropColumn("dbo.BadgesInQuiz", "PublicId");
            DropColumn("dbo.QuizDetails", "PublicId");
        }
    }
}
