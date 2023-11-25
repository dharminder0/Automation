namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ObjectFieldsInAnswer_QuestionId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ObjectFieldsInAnswer", "QuestionId", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ObjectFieldsInAnswer", "QuestionId");
        }
    }
}
