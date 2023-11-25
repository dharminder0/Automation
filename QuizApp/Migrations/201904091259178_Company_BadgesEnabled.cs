namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Company_BadgesEnabled : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Company", "BadgesEnabled", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Company", "BadgesEnabled");
        }
    }
}
