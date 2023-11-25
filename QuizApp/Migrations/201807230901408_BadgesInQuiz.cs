namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class BadgesInQuiz : DbMigration
    {
        public override void Up()
        {
            CreateTable(
              "dbo.BadgesInQuiz",
              c => new
              {
                  Id = c.Int(nullable: false, identity: true),
                  QuizId = c.Int(nullable: false),
                  Title = c.String(),
                  Image = c.String(),
                  AliasTextForNextButton = c.String(),
                  State = c.Int(nullable: false),
                  Status = c.Int(),
                  LastUpdatedOn = c.DateTime(),
                  LastUpdatedBy = c.Int(),
              })
              .PrimaryKey(t => t.Id)
              .ForeignKey("dbo.QuizDetails", t => t.QuizId, cascadeDelete: true)
              .Index(t => t.QuizId);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.BadgesInQuiz", "QuizId", "dbo.QuizDetails");
            DropIndex("dbo.BadgesInQuiz", new[] { "QuizId" });
            DropTable("dbo.BadgesInQuiz");
        }
    }
}
