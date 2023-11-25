namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Question_Content_ShowTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ContentsInQuiz", "ShowTitle", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "ShowTitle", c => c.Boolean(nullable: false));

            Sql("update ContentsInQuiz set ShowTitle = 1");
            Sql("update QuestionsInQuiz set ShowTitle = 1");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuiz", "ShowTitle");
            DropColumn("dbo.ContentsInQuiz", "ShowTitle");
        }
    }
}
