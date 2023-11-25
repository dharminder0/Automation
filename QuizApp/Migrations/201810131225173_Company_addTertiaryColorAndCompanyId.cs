namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Company_addTertiaryColorAndCompanyId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Company", "CompanyId", c => c.Int(nullable: false));
            AddColumn("dbo.Company", "TertiaryColor", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Company", "TertiaryColor");
            DropColumn("dbo.Company", "CompanyId");
        }
    }
}
