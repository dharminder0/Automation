namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Configuration_LeadFormTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfigurationDetails", "LeadFormTitle", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfigurationDetails", "LeadFormTitle");
        }
    }
}
