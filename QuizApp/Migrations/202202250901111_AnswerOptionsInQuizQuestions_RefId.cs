namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AnswerOptionsInQuizQuestions_RefId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "RefId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "RefId");
        }
    }
}
