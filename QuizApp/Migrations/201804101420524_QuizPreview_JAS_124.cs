namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizPreview_JAS_124 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizAttempts", "Mode", c => c.String());
            AddColumn("dbo.QuizStats", "QuizAttemptId", c => c.Int(nullable: false));
            AddColumn("dbo.QuizQuestionStats", "QuizAttemptId", c => c.Int(nullable: false));
            AlterColumn("dbo.QuizAttempts", "LeadUserId", c => c.Int());
            CreateIndex("dbo.QuizQuestionStats", "QuizAttemptId");
            CreateIndex("dbo.QuizStats", "QuizAttemptId");
            AddForeignKey("dbo.QuizQuestionStats", "QuizAttemptId", "dbo.QuizAttempts", "Id", cascadeDelete: false);
            AddForeignKey("dbo.QuizStats", "QuizAttemptId", "dbo.QuizAttempts", "Id", cascadeDelete: true);
            DropColumn("dbo.QuizStats", "QuizCode");
            DropColumn("dbo.QuizQuestionStats", "QuizCode");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuizQuestionStats", "QuizCode", c => c.String());
            AddColumn("dbo.QuizStats", "QuizCode", c => c.String());
            DropForeignKey("dbo.QuizStats", "QuizAttemptId", "dbo.QuizAttempts");
            DropForeignKey("dbo.QuizQuestionStats", "QuizAttemptId", "dbo.QuizAttempts");
            DropIndex("dbo.QuizStats", new[] { "QuizAttemptId" });
            DropIndex("dbo.QuizQuestionStats", new[] { "QuizAttemptId" });
            AlterColumn("dbo.QuizAttempts", "LeadUserId", c => c.Int(nullable: false));
            DropColumn("dbo.QuizQuestionStats", "QuizAttemptId");
            DropColumn("dbo.QuizStats", "QuizAttemptId");
            DropColumn("dbo.QuizAttempts", "Mode");
        }
    }
}
