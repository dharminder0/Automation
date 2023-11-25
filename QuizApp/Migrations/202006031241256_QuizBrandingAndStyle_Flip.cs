namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_Flip : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "Flip", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "Flip");
        }
    }
}
