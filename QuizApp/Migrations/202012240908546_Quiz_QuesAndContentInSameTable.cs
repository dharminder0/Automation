namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_QuesAndContentInSameTable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "ShowQuizCoverTitle", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizDetails", "ShowDescription", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizDetails", "EnableNextButton", c => c.Boolean(nullable: false));
            AddColumn("dbo.BadgesInQuiz", "ShowTitle", c => c.Boolean(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "EnableNextButton", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "EnableNextButton", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "Description", c => c.String());
            AddColumn("dbo.QuestionsInQuiz", "DescriptionImage", c => c.String());
            AddColumn("dbo.QuestionsInQuiz", "EnableMediaFileForDescription", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "PublicIdForDescription", c => c.String());
            AddColumn("dbo.QuestionsInQuiz", "ShowDescriptionImage", c => c.Boolean());
            AddColumn("dbo.QuestionsInQuiz", "AutoPlayForDescription", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "Type", c => c.Int(nullable: false));
            AddColumn("dbo.QuizResults", "ShowExternalTitle", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizResults", "ShowInternalTitle", c => c.Boolean(nullable: false));
            AddColumn("dbo.QuizResults", "ShowDescription", c => c.Boolean(nullable: false));
            AddColumn("dbo.Quiz", "QuesAndContentInSameTable", c => c.Boolean(nullable: false));

            Sql("update QuizDetails set ShowQuizCoverTitle = 1, ShowDescription = 1, EnableNextButton = 1");
            Sql("update BadgesInQuiz set ShowTitle = 1");
            Sql("update ContentsInQuiz set EnableNextButton = 1");
            Sql("update QuestionsInQuiz set EnableNextButton = 1, Type = 2");
            Sql("update QuizResults set ShowExternalTitle = 1, ShowInternalTitle = 1, ShowDescription = 1");
            Sql("update quiz set Quesandcontentinsametable = 1 where quiztype = 5 or quiztype = 6 or quiztype = 7");
        }
        
        public override void Down()
        {
            DropColumn("dbo.Quiz", "QuesAndContentInSameTable");
            DropColumn("dbo.QuizResults", "ShowDescription");
            DropColumn("dbo.QuizResults", "ShowInternalTitle");
            DropColumn("dbo.QuizResults", "ShowExternalTitle");
            DropColumn("dbo.QuestionsInQuiz", "Type");
            DropColumn("dbo.QuestionsInQuiz", "AutoPlayForDescription");
            DropColumn("dbo.QuestionsInQuiz", "ShowDescriptionImage");
            DropColumn("dbo.QuestionsInQuiz", "PublicIdForDescription");
            DropColumn("dbo.QuestionsInQuiz", "EnableMediaFileForDescription");
            DropColumn("dbo.QuestionsInQuiz", "DescriptionImage");
            DropColumn("dbo.QuestionsInQuiz", "Description");
            DropColumn("dbo.QuestionsInQuiz", "EnableNextButton");
            DropColumn("dbo.ContentsInQuiz", "EnableNextButton");
            DropColumn("dbo.BadgesInQuiz", "ShowTitle");
            DropColumn("dbo.QuizDetails", "EnableNextButton");
            DropColumn("dbo.QuizDetails", "ShowDescription");
            DropColumn("dbo.QuizDetails", "ShowQuizCoverTitle");
        }
    }
}
