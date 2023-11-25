namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_Version_RelatedChange : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "Version", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizDetails", "Version");
        }
    }
}
