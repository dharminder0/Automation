namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addMigration_QuizDetails_SecondsToApply : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "SecondsToApply", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizDetails", "SecondsToApply");
        }
    }
}
