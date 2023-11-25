namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiztag_UrlSetting_AnswerStat : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizTagDetails", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.QuizAnswerStats", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.QuizAnswerStats", "QuizAttemptId", c => c.Int(nullable: false));
            AddColumn("dbo.QuizUrlSetting", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizUrlSetting", "CompanyId");
            DropColumn("dbo.QuizAnswerStats", "QuizAttemptId");
            DropColumn("dbo.QuizAnswerStats", "CompanyId");
            DropColumn("dbo.QuizTagDetails", "CompanyId");
        }
    }
}
