namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AnswerStructureType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuiz", "AnswerStructureType", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuiz", "AnswerStructureType");
        }
    }
}
