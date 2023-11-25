namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class companyId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "CompanyId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizDetails", "CompanyId");
        }
    }
}
