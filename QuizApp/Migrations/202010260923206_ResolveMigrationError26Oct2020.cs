namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ResolveMigrationError26Oct2020 : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.ConfigurationDetails", "Test");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ConfigurationDetails", "Test", c => c.String());
        }
    }
}
