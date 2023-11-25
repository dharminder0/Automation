namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_HoverColor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "HoverColor", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "HoverColor");
        }
    }
}
