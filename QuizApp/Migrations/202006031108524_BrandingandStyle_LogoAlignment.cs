namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BrandingandStyle_LogoAlignment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "AutomationAlignment", c => c.String());
            AddColumn("dbo.QuizBrandingAndStyle", "LogoAlignment", c => c.String());
            DropColumn("dbo.QuizBrandingAndStyle", "Alignment");

            Sql("update QuizBrandingAndStyle set AutomationAlignment = 'Center', LogoAlignment = 'Left'");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "Alignment", c => c.String());
            DropColumn("dbo.QuizBrandingAndStyle", "LogoAlignment");
            DropColumn("dbo.QuizBrandingAndStyle", "AutomationAlignment");
        }
    }
}
