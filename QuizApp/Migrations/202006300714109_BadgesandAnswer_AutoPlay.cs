namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BadgesandAnswer_AutoPlay : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BadgesInQuiz", "AutoPlay", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "AutoPlayForDescription", c => c.Boolean(nullable: false));
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "AutoPlay", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "AutoPlay");
            DropColumn("dbo.ContentsInQuiz", "AutoPlayForDescription");
            DropColumn("dbo.BadgesInQuiz", "AutoPlay");
        }
    }
}
