namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_BackgroundColorofLogo_Alignment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "BackgroundColorofLogo", c => c.String());
            AddColumn("dbo.QuizBrandingAndStyle", "Alignment", c => c.String());

            Sql("update QuizBrandingAndStyle set BackgroundColorofLogo = '#FFFFFF', Alignment = 'Left'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "Alignment");
            DropColumn("dbo.QuizBrandingAndStyle", "BackgroundColorofLogo");
        }
    }
}
