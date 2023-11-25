namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AnswerOptionsInQuizQuestions_Description : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "Description", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "Description");
        }
    }
}
