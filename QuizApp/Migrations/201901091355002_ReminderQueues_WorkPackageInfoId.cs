namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReminderQueues_WorkPackageInfoId : DbMigration
    {
        public override void Up()
        {
            Sql("TRUNCATE TABLE ReminderQueues");
            AddColumn("dbo.ReminderQueues", "WorkPackageInfoId", c => c.Int(nullable: false));
            CreateIndex("dbo.ReminderQueues", "WorkPackageInfoId");
            AddForeignKey("dbo.ReminderQueues", "WorkPackageInfoId", "dbo.WorkPackageInfo", "Id", cascadeDelete: true);
            DropColumn("dbo.ReminderQueues", "QuizCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ReminderQueues", "QuizCode", c => c.String());
            DropForeignKey("dbo.ReminderQueues", "WorkPackageInfoId", "dbo.WorkPackageInfo");
            DropIndex("dbo.ReminderQueues", new[] { "WorkPackageInfoId" });
            DropColumn("dbo.ReminderQueues", "WorkPackageInfoId");
        }
    }
}
