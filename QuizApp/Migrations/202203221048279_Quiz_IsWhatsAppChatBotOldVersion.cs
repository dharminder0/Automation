namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_IsWhatsAppChatBotOldVersion : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Quiz", "IsWhatsAppChatBotOldVersion", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quiz", "IsWhatsAppChatBotOldVersion");
        }
    }
}
