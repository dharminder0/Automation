namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MediaVariablesDetails_MediaOwner : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.MediaVariablesDetails", "MediaOwner", c => c.String());
            AddColumn("dbo.MediaVariablesDetails", "ProfileMedia", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.MediaVariablesDetails", "ProfileMedia");
            DropColumn("dbo.MediaVariablesDetails", "MediaOwner");
        }
    }
}
