namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_TechnicalRecruiter : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ModulePermissionsInQuiz",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        ModuleTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
            AddColumn("dbo.Company", "CreateTechnicalRecruiterCourseEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "CreateTechnicalRecruiterCourse", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsAutomationTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsAppointmentTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsELearningTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsCanvasTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsVacanciesTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsContactsTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsReviewTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsReportingTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "IsCampaignsTechnicalRecruiterCoursePermission", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ModulePermissionsInQuiz", "QuizId", "dbo.Quiz");
            DropIndex("dbo.ModulePermissionsInQuiz", new[] { "QuizId" });
            DropColumn("dbo.UserTokens", "IsCampaignsTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "IsReportingTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "IsReviewTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "IsContactsTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "IsVacanciesTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "IsCanvasTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "IsELearningTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "IsAppointmentTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "IsAutomationTechnicalRecruiterCoursePermission");
            DropColumn("dbo.UserTokens", "CreateTechnicalRecruiterCourse");
            DropColumn("dbo.Company", "CreateTechnicalRecruiterCourseEnabled");
            DropTable("dbo.ModulePermissionsInQuiz");
        }
    }
}
