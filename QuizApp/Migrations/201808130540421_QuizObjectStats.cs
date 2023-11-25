namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizObjectStats : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.QuizBadgeStats", "BadgeId", "dbo.BadgesInQuiz");
            DropIndex("dbo.QuizBadgeStats", new[] { "BadgeId" });
            CreateTable(
                "dbo.QuizObjectStats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizAttemptId = c.Int(nullable: false),
                        ObjectId = c.Int(nullable: false),
                        ViewedOn = c.DateTime(nullable: false),
                        TypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.QuizAttempts", t => t.QuizAttemptId, cascadeDelete: true)
                .Index(t => t.QuizAttemptId);
            
            DropTable("dbo.QuizBadgeStats");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.QuizBadgeStats",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizAttemptId = c.Int(nullable: false),
                        BadgeId = c.Int(nullable: false),
                        ViewedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            DropForeignKey("dbo.QuizObjectStats", "QuizAttemptId", "dbo.QuizAttempts");
            DropIndex("dbo.QuizObjectStats", new[] { "QuizAttemptId" });
            DropTable("dbo.QuizObjectStats");
            CreateIndex("dbo.QuizBadgeStats", "BadgeId");
            AddForeignKey("dbo.QuizBadgeStats", "BadgeId", "dbo.BadgesInQuiz", "Id", cascadeDelete: true);
        }
    }
}
