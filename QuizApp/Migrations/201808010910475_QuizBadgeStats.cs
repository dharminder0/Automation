namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBadgeStats : DbMigration
    {
        public override void Up()
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
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BadgesInQuiz", t => t.BadgeId, cascadeDelete: true)
                .Index(t => t.BadgeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.QuizBadgeStats", "BadgeId", "dbo.BadgesInQuiz");
            DropIndex("dbo.QuizBadgeStats", new[] { "BadgeId" });
            DropTable("dbo.QuizBadgeStats");
        }
    }
}
