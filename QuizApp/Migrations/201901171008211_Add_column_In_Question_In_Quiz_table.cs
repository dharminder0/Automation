namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_column_In_Question_In_Quiz_table : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuiz", "MaxAnswer", c => c.Int());
            AddColumn("dbo.QuestionsInQuiz", "MinAnswer", c => c.Int());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuestionsInQuiz", "MinAnswer");
            DropColumn("dbo.QuestionsInQuiz", "MaxAnswer");
        }
    }
}
