namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PublicId_InAttachmentsInQuiz : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.AttachmentsInQuiz", "PublicId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.AttachmentsInQuiz", "PublicId");
        }
    }
}
