namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class WhatsappLogging : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.WhatsappLogging",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ClientCode = c.String(),
                        ContactId = c.String(),
                        ContactPhone = c.String(),
                        ModuleName = c.String(),
                        EventType = c.String(),
                        ObjectId = c.String(),
                        UniqueCode = c.String(),
                        Status = c.String(),
                        CommunicationType = c.String(),
                        ErrorMessage = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.WhatsappLogging");
        }
    }
}
