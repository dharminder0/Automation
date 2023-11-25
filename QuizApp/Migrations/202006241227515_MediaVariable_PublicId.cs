namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MediaVariable_PublicId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MediaVariablesDetails", "ObjectPublicId", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MediaVariablesDetails", "ObjectPublicId");
        }
    }
}
