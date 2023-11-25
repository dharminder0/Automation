namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_ButtonHoverColor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "ButtonHoverColor", c => c.String(maxLength: 50));
            DropColumn("dbo.QuizBrandingAndStyle", "HoverColor");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "HoverColor", c => c.String(maxLength: 50));
            DropColumn("dbo.QuizBrandingAndStyle", "ButtonHoverColor");
        }
    }
}
