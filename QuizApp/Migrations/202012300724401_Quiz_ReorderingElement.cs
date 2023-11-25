namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_ReorderingElement : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuizDetails", "DisplayOrderForTitle", c => c.Int(nullable: false));
            AddColumn("dbo.QuizDetails", "DisplayOrderForTitleImage", c => c.Int(nullable: false));
            AddColumn("dbo.QuizDetails", "DisplayOrderForDescription", c => c.Int(nullable: false));
            AddColumn("dbo.QuizDetails", "DisplayOrderForNextButton", c => c.Int(nullable: false));
            AddColumn("dbo.BadgesInQuiz", "DisplayOrderForTitle", c => c.Int(nullable: false));
            AddColumn("dbo.BadgesInQuiz", "DisplayOrderForTitleImage", c => c.Int(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "DisplayOrderForTitle", c => c.Int(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "DisplayOrderForTitleImage", c => c.Int(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "DisplayOrderForDescription", c => c.Int(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "DisplayOrderForDescriptionImage", c => c.Int(nullable: false));
            AddColumn("dbo.ContentsInQuiz", "DisplayOrderForNextButton", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "DisplayOrderForTitle", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "DisplayOrderForTitleImage", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "DisplayOrderForDescription", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "DisplayOrderForDescriptionImage", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "DisplayOrderForAnswer", c => c.Int(nullable: false));
            AddColumn("dbo.QuestionsInQuiz", "DisplayOrderForNextButton", c => c.Int(nullable: false));
            AddColumn("dbo.QuizResults", "DisplayOrderForTitle", c => c.Int(nullable: false));
            AddColumn("dbo.QuizResults", "DisplayOrderForTitleImage", c => c.Int(nullable: false));
            AddColumn("dbo.QuizResults", "DisplayOrderForDescription", c => c.Int(nullable: false));
            AddColumn("dbo.QuizResults", "DisplayOrderForNextButton", c => c.Int(nullable: false));

            Sql("update QuizDetails set DisplayOrderForTitleImage = 2, DisplayOrderForTitle = 1, DisplayOrderForDescription = 3, DisplayOrderForNextButton = 4");
            Sql("update BadgesInQuiz set DisplayOrderForTitleImage = 2, DisplayOrderForTitle = 1");
            Sql("update ContentsInQuiz set DisplayOrderForTitleImage = 2, DisplayOrderForTitle = 1, DisplayOrderForDescriptionImage = 4, DisplayOrderForDescription = 3, DisplayOrderForNextButton = 5");
            Sql("update QuestionsInQuiz set DisplayOrderForTitleImage = 2, DisplayOrderForTitle = 1, DisplayOrderForDescriptionImage = 4, DisplayOrderForDescription = 3, DisplayOrderForAnswer = 5, DisplayOrderForNextButton = 6");
            Sql("update QuizResults set DisplayOrderForTitleImage = 2, DisplayOrderForTitle = 1, DisplayOrderForDescription = 3, DisplayOrderForNextButton = 4");
        }
        
        public override void Down()
        {
            DropColumn("dbo.QuizResults", "DisplayOrderForNextButton");
            DropColumn("dbo.QuizResults", "DisplayOrderForDescription");
            DropColumn("dbo.QuizResults", "DisplayOrderForTitleImage");
            DropColumn("dbo.QuizResults", "DisplayOrderForTitle");
            DropColumn("dbo.QuestionsInQuiz", "DisplayOrderForNextButton");
            DropColumn("dbo.QuestionsInQuiz", "DisplayOrderForAnswer");
            DropColumn("dbo.QuestionsInQuiz", "DisplayOrderForDescriptionImage");
            DropColumn("dbo.QuestionsInQuiz", "DisplayOrderForDescription");
            DropColumn("dbo.QuestionsInQuiz", "DisplayOrderForTitleImage");
            DropColumn("dbo.QuestionsInQuiz", "DisplayOrderForTitle");
            DropColumn("dbo.ContentsInQuiz", "DisplayOrderForNextButton");
            DropColumn("dbo.ContentsInQuiz", "DisplayOrderForDescriptionImage");
            DropColumn("dbo.ContentsInQuiz", "DisplayOrderForDescription");
            DropColumn("dbo.ContentsInQuiz", "DisplayOrderForTitleImage");
            DropColumn("dbo.ContentsInQuiz", "DisplayOrderForTitle");
            DropColumn("dbo.BadgesInQuiz", "DisplayOrderForTitleImage");
            DropColumn("dbo.BadgesInQuiz", "DisplayOrderForTitle");
            DropColumn("dbo.QuizDetails", "DisplayOrderForNextButton");
            DropColumn("dbo.QuizDetails", "DisplayOrderForDescription");
            DropColumn("dbo.QuizDetails", "DisplayOrderForTitleImage");
            DropColumn("dbo.QuizDetails", "DisplayOrderForTitle");
        }
    }
}
