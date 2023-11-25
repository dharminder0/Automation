namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class VariableInQuiz_QuizDetailId : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.VariableInQuiz", "QuizId");
            AddForeignKey("dbo.VariableInQuiz", "QuizId", "dbo.QuizDetails", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.VariableInQuiz", "QuizId", "dbo.QuizDetails");
            DropIndex("dbo.VariableInQuiz", new[] { "QuizId" });
        }
    }
}
