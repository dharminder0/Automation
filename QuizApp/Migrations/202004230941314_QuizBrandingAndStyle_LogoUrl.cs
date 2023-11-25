namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_LogoUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "LogoUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "LogoUrl");
        }
    }
}
