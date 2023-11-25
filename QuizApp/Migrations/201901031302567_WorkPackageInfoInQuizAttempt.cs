namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkPackageInfoInQuizAttempt : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizAttempts", "WorkPackageInfoId", c => c.Int());
            AddColumn("dbo.WorkPackageInfo", "CreatedOn", c => c.DateTime());
            CreateIndex("dbo.QuizAttempts", "WorkPackageInfoId");
            AddForeignKey("dbo.QuizAttempts", "WorkPackageInfoId", "dbo.WorkPackageInfo", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizAttempts", "WorkPackageInfoId", "dbo.WorkPackageInfo");
            DropIndex("dbo.QuizAttempts", new[] { "WorkPackageInfoId" });
            DropColumn("dbo.WorkPackageInfo", "CreatedOn");
            DropColumn("dbo.QuizAttempts", "WorkPackageInfoId");
        }
    }
}
