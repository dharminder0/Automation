namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionsInQuiz_LanguageId_TemplateId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuiz", "LanguageId", c => c.Int());
            AddColumn("dbo.QuestionsInQuiz", "TemplateId", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuiz", "TemplateId");
            DropColumn("dbo.QuestionsInQuiz", "LanguageId");
        }
    }
}
