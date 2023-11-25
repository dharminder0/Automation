namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Add_CreateStandardAutomationPermission : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTokens", "IsCreateStandardAutomationPermission", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserTokens", "IsCreateStandardAutomationPermission");
        }
    }
}
