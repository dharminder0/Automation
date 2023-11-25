namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BrandingandStyle_Alignment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "BackgroundColorofLogo", c => c.String());
            AddColumn("dbo.QuizBrandingAndStyle", "AutomationAlignment", c => c.String());
            AddColumn("dbo.QuizBrandingAndStyle", "LogoAlignment", c => c.String());

            Sql("update QuizBrandingAndStyle set BackgroundColorofLogo = '#FFFFFF', AutomationAlignment = 'Center', LogoAlignment = 'Left'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "LogoAlignment");
            DropColumn("dbo.QuizBrandingAndStyle", "AutomationAlignment");
            DropColumn("dbo.QuizBrandingAndStyle", "BackgroundColorofLogo");
        }
    }
}
