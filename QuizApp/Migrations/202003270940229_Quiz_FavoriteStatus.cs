namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_FavoriteStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quiz", "IsFavorite", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quiz", "IsFavorite");
        }
    }
}
