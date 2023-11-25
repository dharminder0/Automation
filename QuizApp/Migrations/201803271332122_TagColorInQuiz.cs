namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TagColorInQuiz : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quiz", "TagColor", c => c.String());
            AddColumn("dbo.Quiz", "LabelText", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quiz", "LabelText");
            DropColumn("dbo.Quiz", "TagColor");
        }
    }
}
