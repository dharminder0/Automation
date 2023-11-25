namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class NotificationTemplate_EmailLinkVariable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.NotificationTemplate", "EmailLinkVariable", c => c.String());
            AddColumn("dbo.WorkPackageInfo", "EmailLinkVariableForInvitation", c => c.String());
            AddColumn("dbo.WorkPackageInfo", "ShotenUrlCodeForFirstReminder", c => c.String());
            AddColumn("dbo.WorkPackageInfo", "EmailLinkVariableForFirstReminder", c => c.String());
            AddColumn("dbo.WorkPackageInfo", "ShotenUrlCodeForSecondReminder", c => c.String());
            AddColumn("dbo.WorkPackageInfo", "EmailLinkVariableForSecondReminder", c => c.String());
            AddColumn("dbo.WorkPackageInfo", "ShotenUrlCodeForThirdReminder", c => c.String());
            AddColumn("dbo.WorkPackageInfo", "EmailLinkVariableForThirdReminder", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPackageInfo", "EmailLinkVariableForThirdReminder");
            DropColumn("dbo.WorkPackageInfo", "ShotenUrlCodeForThirdReminder");
            DropColumn("dbo.WorkPackageInfo", "EmailLinkVariableForSecondReminder");
            DropColumn("dbo.WorkPackageInfo", "ShotenUrlCodeForSecondReminder");
            DropColumn("dbo.WorkPackageInfo", "EmailLinkVariableForFirstReminder");
            DropColumn("dbo.WorkPackageInfo", "ShotenUrlCodeForFirstReminder");
            DropColumn("dbo.WorkPackageInfo", "EmailLinkVariableForInvitation");
            DropColumn("dbo.NotificationTemplate", "EmailLinkVariable");
        }
    }
}
