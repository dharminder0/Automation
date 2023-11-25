namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizDetails_UsageType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "UsageType", c => c.Int());
            AddColumn("dbo.QuizTagDetails", "TagId", c => c.Int(nullable: false));
            DropColumn("dbo.QuizTagDetails", "TagColor");
            DropColumn("dbo.QuizTagDetails", "LabelText");

            Sql("update QuizDetails set UsageType = 1");
            Sql("delete from QuizTagDetails");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizTagDetails", "LabelText", c => c.String());
            AddColumn("dbo.QuizTagDetails", "TagColor", c => c.String());
            DropColumn("dbo.QuizTagDetails", "TagId");
            DropColumn("dbo.QuizDetails", "UsageType");
        }
    }
}
