namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PersonalityResultSetting_ShowLeadUserForm : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.PersonalityResultSetting", "ShowLeadUserForm", c => c.Boolean(nullable: false));
            Sql("update PersonalityResultSetting set ShowLeadUserForm = 1");
        }
        
        public override void Down()
        {
            DropColumn("dbo.PersonalityResultSetting", "ShowLeadUserForm");
        }
    }
}
