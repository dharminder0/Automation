namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_ColNameChange : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyles", "LastUpdatedOn", c => c.DateTime());
            DropColumn("dbo.QuizBrandingAndStyles", "LastUpdateOn");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizBrandingAndStyles", "LastUpdateOn", c => c.DateTime());
            DropColumn("dbo.QuizBrandingAndStyles", "LastUpdatedOn");
        }
    }
}
