namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_BackColorAndImage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "IsBackType", c => c.Int(nullable: false));
            AddColumn("dbo.QuizBrandingAndStyle", "BackImageFileURL", c => c.String());
            AddColumn("dbo.QuizBrandingAndStyle", "BackColor", c => c.String());
            AddColumn("dbo.QuizBrandingAndStyle", "Opacity", c => c.String());
            Sql("update QuizBrandingAndStyle set IsBackType = 2, BackColor ='#ffffff',Opacity ='rgba(255, 255, 255, 0)'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "Opacity");
            DropColumn("dbo.QuizBrandingAndStyle", "BackColor");
            DropColumn("dbo.QuizBrandingAndStyle", "BackImageFileURL");
            DropColumn("dbo.QuizBrandingAndStyle", "IsBackType");
        }
    }
}
