namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class QuizTablePluralizeRemove : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.QuestionsInQuizs", newName: "QuestionsInQuiz");
            RenameTable(name: "dbo.Quizs", newName: "Quiz");
            RenameTable(name: "dbo.NotificationTemplatesInQuizs", newName: "NotificationTemplatesInQuiz");
            RenameTable(name: "dbo.NotificationTemplates", newName: "NotificationTemplate");
            RenameTable(name: "dbo.NotificationTemplatesInResults", newName: "NotificationTemplatesInResult");
            RenameTable(name: "dbo.UserAccessInQuizs", newName: "UserAccessInQuiz");
            RenameTable(name: "dbo.QuizBrandingAndStyles", newName: "QuizBrandingAndStyle");
            RenameTable(name: "dbo.QuizBranchingLogics", newName: "QuizBranchingLogic");
            RenameTable(name: "dbo.RemindersInQuizs", newName: "RemindersInQuiz");
        }
        
        public override void Down()
        {
            RenameTable(name: "dbo.RemindersInQuiz", newName: "RemindersInQuizs");
            RenameTable(name: "dbo.QuizBranchingLogic", newName: "QuizBranchingLogics");
            RenameTable(name: "dbo.QuizBrandingAndStyle", newName: "QuizBrandingAndStyles");
            RenameTable(name: "dbo.UserAccessInQuiz", newName: "UserAccessInQuizs");
            RenameTable(name: "dbo.NotificationTemplatesInResult", newName: "NotificationTemplatesInResults");
            RenameTable(name: "dbo.NotificationTemplate", newName: "NotificationTemplates");
            RenameTable(name: "dbo.NotificationTemplatesInQuiz", newName: "NotificationTemplatesInQuizs");
            RenameTable(name: "dbo.Quiz", newName: "Quizs");
            RenameTable(name: "dbo.QuestionsInQuiz", newName: "QuestionsInQuizs");
        }
    }
}
