namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AttemptQuizLog1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.AttemptQuizLog",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        LeadId = c.String(),
                        QuizAttemptId = c.Int(nullable: false),
                        RequestJson = c.String(),
                        CreatedOn = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.AttemptQuizLog");
        }
    }
}
