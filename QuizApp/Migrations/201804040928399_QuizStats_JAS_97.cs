namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizStats_JAS_97 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizStats", "ResultId", c => c.Int());
            CreateIndex("dbo.QuizStats", "ResultId");
            AddForeignKey("dbo.QuizStats", "ResultId", "dbo.QuizResults", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizStats", "ResultId", "dbo.QuizResults");
            DropIndex("dbo.QuizStats", new[] { "ResultId" });
            DropColumn("dbo.QuizStats", "ResultId");
        }
    }
}
