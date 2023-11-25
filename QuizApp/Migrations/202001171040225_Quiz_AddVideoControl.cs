namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_AddVideoControl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "VideoControls", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "VideoControls", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "VideoControls", c => c.Boolean(nullable: false));
            Sql("update dbo.QuizDetails set VideoControls = 1");
            Sql("update dbo.ContentsInQuiz set VideoControls = 1");
            Sql("update dbo.QuestionsInQuiz set VideoControls = 1");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuiz", "VideoControls");
            DropColumn("dbo.ContentsInQuiz", "VideoControls");
            DropColumn("dbo.QuizDetails", "VideoControls");
        }
    }
}
