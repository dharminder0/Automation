namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AttemptQuizLog_ResponseJson : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AttemptQuizLog", "ResponseJson", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AttemptQuizLog", "ResponseJson");
        }
    }
}
