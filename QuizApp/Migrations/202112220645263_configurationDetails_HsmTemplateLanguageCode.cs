namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class configurationDetails_HsmTemplateLanguageCode : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.ConfigurationDetails", "HsmTemplateLanguageCode", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.ConfigurationDetails", "HsmTemplateLanguageCode");
        }
    }
}
