namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizResult_InternalTitle : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizResults", "InternalTitle", c => c.String(maxLength: 200));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "InternalTitle");
        }
    }
}
