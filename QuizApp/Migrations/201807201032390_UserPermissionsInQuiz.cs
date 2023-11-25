namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class UserPermissionsInQuiz : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserPermissionsInQuiz",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        UserTypeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .Index(t => t.QuizId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UserPermissionsInQuiz", "QuizId", "dbo.Quiz");
            DropIndex("dbo.UserPermissionsInQuiz", new[] { "QuizId" });
            DropTable("dbo.UserPermissionsInQuiz");
        }
    }
}
