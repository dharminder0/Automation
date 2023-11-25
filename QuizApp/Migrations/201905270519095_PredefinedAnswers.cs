namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PredefinedAnswers : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "IsReadOnly", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizAnswerStats", "SubAnswerTypeId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizAnswerStats", "SubAnswerTypeId");
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "IsReadOnly");
        }
    }
}
