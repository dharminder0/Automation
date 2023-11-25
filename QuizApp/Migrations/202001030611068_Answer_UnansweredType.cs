namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Answer_UnansweredType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.QuestionsInQuiz", "Time", c => c.String());
            AddColumn("dbo.AnswerOptionsInQuizQuestions", "IsUnansweredType", c => c.Boolean(nullable: false));
            DropColumn("dbo.QuestionsInQuiz", "TimeInSecond");
            Sql("insert into AnswerOptionsInQuizQuestions select id, 'Unanswered',null,0,0,getdate(),0,0,1,1,null,null,0,1 from QuestionsInQuiz where status = 1 and quizid in (select id from quizdetails where ParentQuizId in (select id from quiz where quiztype= 4))");
            Sql("insert into AnswerOptionsInQuizQuestions select id, 'Unanswered',null,null,0,getdate(),0,0,1,1,null,null,0,1 from QuestionsInQuiz where status = 1 and quizid in (select id from quizdetails where ParentQuizId in (select id from quiz where quiztype= 2 or quiztype= 3))");
        }
        
        public override void Down()
        {
            AddColumn("dbo.QuestionsInQuiz", "TimeInSecond", c => c.Int());
            DropColumn("dbo.AnswerOptionsInQuizQuestions", "IsUnansweredType");
            DropColumn("dbo.QuestionsInQuiz", "Time");
            Sql("delete from QuizAnswerStats where answerid in(select id from AnswerOptionsInQuizQuestions where IsUnansweredType=1)");
            Sql("delete from AnswerOptionsInQuizQuestions where IsUnansweredType=1");
        }
    }
}
