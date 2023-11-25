namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuestionsInQuiz_LanguageCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuiz", "LanguageCode", c => c.String());
            DropColumn("dbo.QuestionsInQuiz", "LanguageId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuestionsInQuiz", "LanguageId", c => c.Int());
            DropColumn("dbo.QuestionsInQuiz", "LanguageCode");
        }
    }
}
