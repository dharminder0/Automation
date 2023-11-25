namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_ButtonHoverTextColor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "ButtonHoverTextColor", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "ButtonHoverTextColor");
        }
    }
}
