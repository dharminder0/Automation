namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VideoFrameUpdate : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.AnswerOptionsInQuizQuestions", "DescVideoFrameEnabled", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.AnswerOptionsInQuizQuestions", "DescVideoFrameEnabled", c => c.Boolean(nullable: false));
        }
    }
}
