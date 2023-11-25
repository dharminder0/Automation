namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkPackageInfo_RequestId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPackageInfo", "RequestId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPackageInfo", "RequestId");
        }
    }
}
