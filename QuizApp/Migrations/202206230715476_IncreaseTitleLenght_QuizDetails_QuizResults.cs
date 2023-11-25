namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class IncreaseTitleLenght_QuizDetails_QuizResults : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.QuizDetails", "QuizCoverTitle", c => c.String(maxLength: 500));
            AlterColumn("dbo.QuizResults", "Title", c => c.String(maxLength: 500));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.QuizResults", "Title", c => c.String(maxLength: 200));
            AlterColumn("dbo.QuizDetails", "QuizCoverTitle", c => c.String(maxLength: 200));
        }
    }
}
