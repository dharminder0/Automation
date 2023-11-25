namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_FavoriteQuizByUser : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FavoriteQuizByUser",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        QuizId = c.Int(nullable: false),
                        UserTokenId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Quiz", t => t.QuizId, cascadeDelete: true)
                .ForeignKey("dbo.UserTokens", t => t.UserTokenId, cascadeDelete: true)
                .Index(t => t.QuizId)
                .Index(t => t.UserTokenId);
            
            DropColumn("dbo.Quiz", "IsFavorite");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Quiz", "IsFavorite", c => c.Boolean(nullable: false));
            DropForeignKey("dbo.FavoriteQuizByUser", "UserTokenId", "dbo.UserTokens");
            DropForeignKey("dbo.FavoriteQuizByUser", "QuizId", "dbo.Quiz");
            DropIndex("dbo.FavoriteQuizByUser", new[] { "UserTokenId" });
            DropIndex("dbo.FavoriteQuizByUser", new[] { "QuizId" });
            DropTable("dbo.FavoriteQuizByUser");
        }
    }
}
