namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_JAS_103 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuizs", "ShowAnswerImage", c => c.Boolean());
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "ShowAnswerImage");
            DropColumn("dbo.QuestionsInQuizs", "SetCorrectAnswer");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuestionsInQuizs", "SetCorrectAnswer", c => c.Boolean());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "ShowAnswerImage", c => c.Boolean());
            DropColumn("dbo.QuestionsInQuizs", "ShowAnswerImage");
        }
    }
}
