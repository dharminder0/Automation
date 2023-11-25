namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BrandingAndStyle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "ButtonFontColor", c => c.String(maxLength: 50));
            AddColumn("dbo.QuizBrandingAndStyle", "OptionFontColor", c => c.String(maxLength: 50));
            Sql("Update QuizBrandingAndStyle set ButtonFontColor = '#000000', OptionFontColor = '#000000'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "OptionFontColor");
            DropColumn("dbo.QuizBrandingAndStyle", "ButtonFontColor");
        }
    }
}
