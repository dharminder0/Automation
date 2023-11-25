namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_Language : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "Language", c => c.Int());
            Sql("update QuizBrandingAndStyle set Language = 1");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "Language");
        }
    }
}
