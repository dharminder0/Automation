namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_UsageTypeInQuiz : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UsageTypeInQuiz",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UsageType = c.Int(nullable: false),
                        QuizId = c.Int(nullable: false),
                        LastUpdatedOn = c.DateTime(),
                        LastUpdatedBy = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            DropColumn("dbo.QuizDetails", "UsageType");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizDetails", "UsageType", c => c.Int());
            DropForeignKey("dbo.UsageTypeInQuiz", "QuizId", "dbo.Quiz");
            DropIndex("dbo.UsageTypeInQuiz", new[] { "QuizId" });
            DropTable("dbo.UsageTypeInQuiz");
        }
    }
}
