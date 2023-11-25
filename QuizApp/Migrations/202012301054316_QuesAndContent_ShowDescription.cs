namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuesAndContent_ShowDescription : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ContentsInQuiz", "ShowDescription", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "ShowDescription", c => c.Boolean(nullable: false));

            Sql("update ContentsInQuiz set ShowDescription = 1");
            Sql("update QuestionsInQuiz set ShowDescription = 1");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuiz", "ShowDescription");
            DropColumn("dbo.ContentsInQuiz", "ShowDescription");
        }
    }
}
