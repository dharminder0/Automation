namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ExternalActionQueue : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ExternalActionQueue",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AddedOn = c.DateTime(nullable: false),
                        CompanyId = c.Int(nullable: false),
                        ObjectId = c.String(),
                        ItemType = c.String(),
                        ObjectJson = c.String(),
                        Status = c.Int(nullable: false),
                        ModifiedOn = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.ExternalActionQueue");
        }
    }
}
