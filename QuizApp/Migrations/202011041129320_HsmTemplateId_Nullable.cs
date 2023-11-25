namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class HsmTemplateId_Nullable : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ConfigurationDetails", "HsmTemplateId", c => c.Int());
            Sql("update configurationdetails set HsmTemplateId = NULL");
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ConfigurationDetails", "HsmTemplateId", c => c.Int(nullable: false));
        }
    }
}
