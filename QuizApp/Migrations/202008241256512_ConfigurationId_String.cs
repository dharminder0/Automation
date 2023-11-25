namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigurationId_String : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ConfigurationDetails", "ConfigurationId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ConfigurationDetails", "ConfigurationId", c => c.Long(nullable: false));
        }
    }
}
