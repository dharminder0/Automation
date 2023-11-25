namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_CoverImageColumns : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quizs", "QuizCoverImgAttributionLabel", c => c.String());
            AddColumn("dbo.Quizs", "QuizCoverImgAltTag", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quizs", "QuizCoverImgAltTag");
            DropColumn("dbo.Quizs", "QuizCoverImgAttributionLabel");
        }
    }
}
