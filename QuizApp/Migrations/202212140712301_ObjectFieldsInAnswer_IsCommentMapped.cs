namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ObjectFieldsInAnswer_IsCommentMapped : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ObjectFieldsInAnswer", "IsCommentMapped", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.ObjectFieldsInAnswer", "IsCommentMapped");
        }
    }
}
