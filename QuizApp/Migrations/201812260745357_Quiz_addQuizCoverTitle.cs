namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_addQuizCoverTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "QuizCoverTitle", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizDetails", "QuizCoverTitle");
        }
    }
}
