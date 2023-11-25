namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Dynamic_LeadForm : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ResultIdsInConfigurationDetails", "FormId", c => c.Int(nullable: false));
            AddColumn("dbo.ResultIdsInConfigurationDetails", "FlowOrder", c => c.Int(nullable: false));

            Sql("update ResultIdsInConfigurationDetails set FormId = 1, FlowOrder = 3");
        }
        
        public override void Down()
        {
            DropColumn("dbo.ResultIdsInConfigurationDetails", "FlowOrder");
            DropColumn("dbo.ResultIdsInConfigurationDetails", "FormId");
        }
    }
}
