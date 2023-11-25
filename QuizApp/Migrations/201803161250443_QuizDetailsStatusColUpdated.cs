namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizDetailsStatusColUpdated : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.QuizDetails", "Status", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            AlterColumn("dbo.QuizDetails", "Status", c => c.Int());
        }
    }
}
