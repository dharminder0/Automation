namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizVariablesMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.QuizVariables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ObjectTypes = c.Int(nullable: false),
                        Variables = c.String(maxLength: 4000),
                        ObjectId = c.Int(nullable: false),
                        QuizDetailsId = c.Int(nullable: false),
                        CompanyId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.QuizVariables");
        }
    }
}
