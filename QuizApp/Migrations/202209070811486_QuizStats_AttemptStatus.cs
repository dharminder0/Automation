namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizStats_AttemptStatus : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizStats", "AttemptStatus", c => c.Byte());
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizStats", "AttemptStatus");
        }
    }
}
