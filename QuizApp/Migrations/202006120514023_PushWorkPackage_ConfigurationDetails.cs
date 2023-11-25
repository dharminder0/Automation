namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PushWorkPackage_ConfigurationDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ConfigurationDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        ConfigurationId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            AddColumn("dbo.LeadDataInAction", "ConfigurationDetailsId", c => c.Int());
            AddColumn("dbo.WorkPackageInfo", "UpdatedOn", c => c.DateTime());
            AddColumn("dbo.WorkPackageInfo", "ConfigurationDetailsId", c => c.Int());
            AddColumn("dbo.QuizAttempts", "ConfigurationDetailsId", c => c.Int());
            AddColumn("dbo.VariablesDetails", "ConfigurationDetailsId", c => c.Int());
            CreateIndex("dbo.LeadDataInAction", "ConfigurationDetailsId");
            CreateIndex("dbo.WorkPackageInfo", "ConfigurationDetailsId");
            CreateIndex("dbo.QuizAttempts", "ConfigurationDetailsId");
            CreateIndex("dbo.VariablesDetails", "ConfigurationDetailsId");
            AddForeignKey("dbo.WorkPackageInfo", "ConfigurationDetailsId", "dbo.ConfigurationDetails", "Id");
            AddForeignKey("dbo.QuizAttempts", "ConfigurationDetailsId", "dbo.ConfigurationDetails", "Id");
            AddForeignKey("dbo.VariablesDetails", "ConfigurationDetailsId", "dbo.ConfigurationDetails", "Id");
            AddForeignKey("dbo.LeadDataInAction", "ConfigurationDetailsId", "dbo.ConfigurationDetails", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LeadDataInAction", "ConfigurationDetailsId", "dbo.ConfigurationDetails");
            DropForeignKey("dbo.ConfigurationDetails", "QuizId", "dbo.Quiz");
            DropForeignKey("dbo.VariablesDetails", "ConfigurationDetailsId", "dbo.ConfigurationDetails");
            DropForeignKey("dbo.QuizAttempts", "ConfigurationDetailsId", "dbo.ConfigurationDetails");
            DropForeignKey("dbo.WorkPackageInfo", "ConfigurationDetailsId", "dbo.ConfigurationDetails");
            DropIndex("dbo.VariablesDetails", new[] { "ConfigurationDetailsId" });
            DropIndex("dbo.QuizAttempts", new[] { "ConfigurationDetailsId" });
            DropIndex("dbo.WorkPackageInfo", new[] { "ConfigurationDetailsId" });
            DropIndex("dbo.ConfigurationDetails", new[] { "QuizId" });
            DropIndex("dbo.LeadDataInAction", new[] { "ConfigurationDetailsId" });
            DropColumn("dbo.VariablesDetails", "ConfigurationDetailsId");
            DropColumn("dbo.QuizAttempts", "ConfigurationDetailsId");
            DropColumn("dbo.WorkPackageInfo", "ConfigurationDetailsId");
            DropColumn("dbo.WorkPackageInfo", "UpdatedOn");
            DropColumn("dbo.LeadDataInAction", "ConfigurationDetailsId");
            DropTable("dbo.ConfigurationDetails");
        }
    }
}
