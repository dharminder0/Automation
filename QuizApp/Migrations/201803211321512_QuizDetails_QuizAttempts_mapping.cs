namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizDetails_QuizAttempts_mapping : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuizAttempts", "QuizId", "dbo.Quiz");
            AddForeignKey("dbo.QuizAttempts", "QuizId", "dbo.QuizDetails", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizAttempts", "QuizId", "dbo.QuizDetails");
            AddForeignKey("dbo.QuizAttempts", "QuizId", "dbo.Quiz", "Id", cascadeDelete: true);
        }
    }
}
