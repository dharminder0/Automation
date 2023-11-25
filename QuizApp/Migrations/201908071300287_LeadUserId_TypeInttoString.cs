namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LeadUserId_TypeInttoString : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.LeadDataInAction", "LeadUserId", c => c.String());
            AlterColumn("dbo.WorkPackageInfo", "LeadUserId", c => c.String());
            AlterColumn("dbo.QuizAttempts", "LeadUserId", c => c.String());
            AlterColumn("dbo.VariablesDetails", "LeadId", c => c.String());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.VariablesDetails", "LeadId", c => c.Int(nullable: false));
            AlterColumn("dbo.QuizAttempts", "LeadUserId", c => c.Int());
            AlterColumn("dbo.WorkPackageInfo", "LeadUserId", c => c.Int(nullable: false));
            AlterColumn("dbo.LeadDataInAction", "LeadUserId", c => c.Int(nullable: false));
        }
    }
}
