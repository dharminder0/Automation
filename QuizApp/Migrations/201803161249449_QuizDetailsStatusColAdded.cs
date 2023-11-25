namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizDetailsStatusColAdded : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quiz", "State", c => c.Int(nullable: false));
            AddColumn("dbo.QuizDetails", "Status", c => c.Int());
            DropColumn("dbo.Quiz", "Status");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Quiz", "Status", c => c.Int(nullable: false));
            DropColumn("dbo.QuizDetails", "Status");
            DropColumn("dbo.Quiz", "State");
        }
    }
}
