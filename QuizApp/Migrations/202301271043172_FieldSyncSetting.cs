namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FieldSyncSetting : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.FieldSyncSetting",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FieldType = c.String(),
                        FieldOptionName = c.String(),
                        FieldOptionTitle = c.String(),
                        FieldFormula = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.FieldSyncSetting");
        }
    }
}
