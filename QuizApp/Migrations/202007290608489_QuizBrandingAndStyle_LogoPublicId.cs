namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_LogoPublicId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "LogoPublicId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "LogoPublicId");
        }
    }
}
