namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WorkPackageInfo_ShotenUrlCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.WorkPackageInfo", "ShotenUrlCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.WorkPackageInfo", "ShotenUrlCode");
        }
    }
}
