namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Result_ShowLeadUserForm : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizResults", "ShowLeadUserForm", c => c.Boolean(nullable: false));
            Sql("update QuizResults set ShowLeadUserForm = 1");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "ShowLeadUserForm");
        }
    }
}
