namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReminderSetting_CompanyId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.RemindersInQuiz", "CompanyId", c => c.Int());
            CreateIndex("dbo.RemindersInQuiz", "CompanyId");
            AddForeignKey("dbo.RemindersInQuiz", "CompanyId", "dbo.Company", "Id");
            Sql("update RemindersInQuiz set CompanyId  = (select CompanyId from UserTokens where BusinessUserId = RemindersInQuiz.CreatedBy)");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.RemindersInQuiz", "CompanyId", "dbo.Company");
            DropIndex("dbo.RemindersInQuiz", new[] { "CompanyId" });
            DropColumn("dbo.RemindersInQuiz", "CompanyId");
        }
    }
}
