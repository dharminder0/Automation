namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ObjectFieldsInAnswerComment_IsCommentMapped : DbMigration
    {
        public override void Up()
        {
            AlterColumn("dbo.ObjectFieldsInAnswer", "IsCommentMapped", c => c.Boolean());
        }
        
        public override void Down()
        {
            AlterColumn("dbo.ObjectFieldsInAnswer", "IsCommentMapped", c => c.Boolean(nullable: false));
        }
    }
}
