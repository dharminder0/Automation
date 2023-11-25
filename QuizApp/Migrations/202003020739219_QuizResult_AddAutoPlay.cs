namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizResult_AddAutoPlay : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "AutoPlay", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "AutoPlay", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "AutoPlay", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizResults", "AutoPlay", c => c.Boolean(nullable: false));
            DropColumn("dbo.QuizDetails", "VideoControls");
            DropColumn("dbo.ContentsInQuiz", "VideoControls");
            DropColumn("dbo.QuestionsInQuiz", "VideoControls");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuestionsInQuiz", "VideoControls", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "VideoControls", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizDetails", "VideoControls", c => c.Boolean(nullable: false));
            DropColumn("dbo.QuizResults", "AutoPlay");
            DropColumn("dbo.QuestionsInQuiz", "AutoPlay");
            DropColumn("dbo.ContentsInQuiz", "AutoPlay");
            DropColumn("dbo.QuizDetails", "AutoPlay");
        }
    }
}
