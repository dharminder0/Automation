namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Test : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfigurationDetails", "Test", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfigurationDetails", "Test");
        }
    }
}
