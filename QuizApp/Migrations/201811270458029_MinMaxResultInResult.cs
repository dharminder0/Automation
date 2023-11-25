namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MinMaxResultInResult : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizResults", "MinScore", c => c.Int());
            AddColumn("dbo.QuizResults", "MaxScore", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "MaxScore");
            DropColumn("dbo.QuizResults", "MinScore");
        }
    }
}
