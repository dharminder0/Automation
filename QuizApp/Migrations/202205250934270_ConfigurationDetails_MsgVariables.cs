namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConfigurationDetails_MsgVariables : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfigurationDetails", "MsgVariables", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfigurationDetails", "MsgVariables");
        }
    }
}
