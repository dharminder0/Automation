namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MediaVariablesDetails : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.MediaVariablesDetails",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ObjectId = c.Int(nullable: false),
                        ObjectTypeId = c.Int(nullable: false),
                        ObjectValue = c.String(),
                        Type = c.Int(nullable: false),
                        ConfigurationDetailsId = c.Int(),
                        QuizId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.ConfigurationDetails", t => t.ConfigurationDetailsId)
                .ForeignKey("dbo.QuizDetails", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.ConfigurationDetailsId)
                .Index(t => t.QuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.MediaVariablesDetails", "QuizId", "dbo.QuizDetails");
            DropForeignKey("dbo.MediaVariablesDetails", "ConfigurationDetailsId", "dbo.ConfigurationDetails");
            DropIndex("dbo.MediaVariablesDetails", new[] { "QuizId" });
            DropIndex("dbo.MediaVariablesDetails", new[] { "ConfigurationDetailsId" });
            DropTable("dbo.MediaVariablesDetails");
        }
    }
}
