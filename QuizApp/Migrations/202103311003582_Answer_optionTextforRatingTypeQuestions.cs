namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Answer_optionTextforRatingTypeQuestions : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingOne", c => c.String());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingTwo", c => c.String());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingThree", c => c.String());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingFour", c => c.String());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingFive", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingFive");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingFour");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingThree");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingTwo");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "OptionTextforRatingOne");
        }
    }
}
