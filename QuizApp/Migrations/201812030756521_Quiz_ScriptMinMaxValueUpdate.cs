namespace QuizApp.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Quiz_ScriptMinMaxValueUpdate : DbMigration
    {
        public override void Up()
        {
            Sql("DECLARE db_cursorForQuiz CURSOR FOR SELECT Id FROM QuizDetails where status = 1  and  state = 1; DECLARE @Id int;OPEN db_cursorForQuiz;FETCH NEXT FROM db_cursorForQuiz INTO @Id;WHILE @@FETCH_STATUS = 0 BEGIN DECLARE @questionStartCount int = 0; DECLARE @questionEndCount int = -1; DECLARE @resultLength int; select  @resultLength = count(*) from QuizResults where quizid= @Id and Status = 1 and  state = 1; DECLARE @questionLength int;select @questionLength = count(*) + 1 from QuestionsInQuiz where quizid= @Id and Status = 1 and  state = 1; Declare @i int; Declare @Counter int = 1;set @i = @resultLength;DECLARE db_cursorForResult CURSOR FOR SELECT Id from QuizResults where quizid= @Id and Status = 1 and  state = 1; DECLARE @IdResult int;OPEN db_cursorForResult;FETCH NEXT FROM db_cursorForResult INTO @IdResult;WHILE @@FETCH_STATUS = 0 BEGIN ; FETCH NEXT FROM db_cursorForResult INTO @IdResult;if (@i >= 1 AND @questionLength > 0) Begin;declare @tempQuestionCount int = CEILING(CAST(@questionLength AS DECIMAL)/@i) ; set @questionStartCount = @questionEndCount + 1;set @questionEndCount = ( @questionStartCount + @tempQuestionCount - 1);;WITH T AS (SELECT *, DENSE_RANK() OVER (ORDER BY Id ASC) AS Rnk FROM QuizResults where quizid= @Id and Status = 1 and  state = 1) update T set MinScore = @questionStartCount, MaxScore = @questionEndCount WHERE Rnk=@Counter set @questionLength = @questionLength - @tempQuestionCount;end; set  @i =  @i -1;set  @Counter = @Counter +1;END;CLOSE db_cursorForResult;DEALLOCATE db_cursorForResult;FETCH NEXT FROM db_cursorForQuiz INTO @Id;END;CLOSE db_cursorForQuiz;DEALLOCATE db_cursorForQuiz; ");
        }
        
        public override void Down()
        {
        }
    }
}
