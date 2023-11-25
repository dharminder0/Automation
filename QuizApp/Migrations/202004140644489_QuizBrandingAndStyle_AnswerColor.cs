namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizBrandingAndStyle_AnswerColor : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizBrandingAndStyle", "BackgroundColorofSelectedAnswer", c => c.String(maxLength: 50));
            AddColumn("dbo.QuizBrandingAndStyle", "BackgroundColorofAnsweronHover", c => c.String(maxLength: 50));
            AddColumn("dbo.QuizBrandingAndStyle", "AnswerTextColorofSelectedAnswer", c => c.String(maxLength: 50));

            Sql("update QuizBrandingAndStyle set BackgroundColorofSelectedAnswer = '#00B7AB', BackgroundColorofAnsweronHover = '#F9F9F9', AnswerTextColorofSelectedAnswer = '#FFFFFF'");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizBrandingAndStyle", "AnswerTextColorofSelectedAnswer");
            DropColumn("dbo.QuizBrandingAndStyle", "BackgroundColorofAnsweronHover");
            DropColumn("dbo.QuizBrandingAndStyle", "BackgroundColorofSelectedAnswer");
        }
    }
}
