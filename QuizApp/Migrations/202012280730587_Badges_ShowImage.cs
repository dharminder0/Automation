namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Badges_ShowImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "ShowQuizCoverImage", c => c.Boolean(nullable: false));
            AddColumn("dbo.BadgesInQuiz", "ShowImage", c => c.Boolean(nullable: false));

            Sql("update QuizDetails set ShowQuizCoverImage = 1");
            Sql("update BadgesInQuiz set ShowImage = 1");
        }
        
        public override void Down()
        {
            DropColumn("dbo.BadgesInQuiz", "ShowImage");
            DropColumn("dbo.QuizDetails", "ShowQuizCoverImage");
        }
    }
}
