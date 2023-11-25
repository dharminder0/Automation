namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CompanyIdNull : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.QuizDetails", "CompanyId", c => c.Int());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.QuizDetails", "CompanyId", c => c.Int(nullable: false));
        }
    }
}
