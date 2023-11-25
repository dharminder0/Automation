namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class JobRockAcademyAndTempaltePermission : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Company", "CreateAcademyCourseEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.Company", "CreateTemplateEnabled", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "CreateAcademyCourse", c => c.Boolean(nullable: false));
            AddColumn("dbo.UserTokens", "CreateTemplate", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserTokens", "CreateTemplate");
            DropColumn("dbo.UserTokens", "CreateAcademyCourse");
            DropColumn("dbo.Company", "CreateTemplateEnabled");
            DropColumn("dbo.Company", "CreateAcademyCourseEnabled");
        }
    }
}
