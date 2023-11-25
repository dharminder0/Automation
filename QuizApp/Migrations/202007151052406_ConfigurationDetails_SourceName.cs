namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigurationDetails_SourceName : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfigurationDetails", "SourceName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfigurationDetails", "SourceName");
        }
    }
}
