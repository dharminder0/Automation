namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizUrlSetting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizUrlSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        DomainName = c.String(),
                        Key = c.String(),
                        Value = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.WorkPackageInfo", "IsOurEndLogicForInvitation", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkPackageInfo", "IsOurEndLogicForFirstReminder", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkPackageInfo", "IsOurEndLogicForSecondReminder", c => c.Boolean(nullable: false));
            AddColumn("dbo.WorkPackageInfo", "IsOurEndLogicForThirdReminder", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPackageInfo", "IsOurEndLogicForThirdReminder");
            DropColumn("dbo.WorkPackageInfo", "IsOurEndLogicForSecondReminder");
            DropColumn("dbo.WorkPackageInfo", "IsOurEndLogicForFirstReminder");
            DropColumn("dbo.WorkPackageInfo", "IsOurEndLogicForInvitation");
            DropTable("dbo.QuizUrlSetting");
        }
    }
}
