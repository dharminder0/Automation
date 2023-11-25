namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ObjectFieldsInAnswer_IsExternalSync : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ObjectFieldsInAnswer", "IsExternalSync", c => c.Boolean());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ObjectFieldsInAnswer", "IsExternalSync");
        }
    }
}
