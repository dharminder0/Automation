namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizReporting_JAS_189 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizAttempts", "LeadConvertedOn", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizAttempts", "LeadConvertedOn");
        }
    }
}
