namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class User_GlobalOfficeAdmin : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserTokens", "IsGlobalOfficeAdmin", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserTokens", "IsGlobalOfficeAdmin");
        }
    }
}
