namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_JAS_103_StatusForQuesAns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "Status", c => c.Int());
            AddColumn("dbo.QuestionsInQuizs", "Status", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuizs", "Status");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "Status");
        }
    }
}
