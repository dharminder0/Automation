namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizIdAnswerOptionInQuiz : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "QuizId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "QuizId");
        }
    }
}
