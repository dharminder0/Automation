namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizComponentLogs : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizComponentLogs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        DraftedObjectId = c.Int(nullable: false),
                        PublishedObjectId = c.Int(nullable: false),
                        ObjectTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizDetails", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizComponentLogs", "QuizId", "dbo.QuizDetails");
            DropIndex("dbo.QuizComponentLogs", new[] { "QuizId" });
            DropTable("dbo.QuizComponentLogs");
        }
    }
}
