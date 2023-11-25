using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service
{
    public class QuizAttemptLogService
    {
        public void ClearOldQuizAttempt(int quizAttemptId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var dateTime = DateTime.UtcNow.AddDays(-720);
                if (quizAttemptId != null)
                {
                    var quizObj = UOWObj.QuizAttemptsRepository.Get(r => r.Id == quizAttemptId).FirstOrDefault();
                    //var quizDetails = UOWObj.QuizDetailsRepository.Get(v => v.ParentQuizId == quizAttemptId && v.CreatedOn < dateTime).FirstOrDefault();
                    if (quizObj != null)
                    {
                        var quizDetails = UOWObj.QuizDetailsRepository.Get(v => v.ParentQuizId == quizObj.QuizId && v.CreatedOn < dateTime);
                        //var quizObj = UOWObj.QuizAttemptsRepository.Get(r => r.Id == quizDetails.ParentQuizId);
                        foreach (var item in quizDetails.Where(v => v.ParentQuizId != quizObj.QuizId))
                        {
                            int quizId = item.Id;
                            var questionInQuiz = UOWObj.QuestionsInQuizRepository.Get(v => v.QuizId == quizId).FirstOrDefault();
                           if(questionInQuiz != null)
                           {
                                var answerOptionsInQuiz = UOWObj.AnswerOptionsInQuizQuestionsRepository.Get(v => v.QuestionId == questionInQuiz.Id);
                                foreach(var answeroptionsquiz in answerOptionsInQuiz)
                                {
                                    UOWObj.AnswerOptionsInQuizQuestionsRepository.Delete(item);
                                }
                                UOWObj.QuestionsInQuizRepository.Delete(item);
                           }
                        }
                    }
                }
            }
        }


        private string DeleteQuizAttempt(int attemptId, AutomationUnitOfWork UOWObj)
        {
            var query = $@" 
SELECT t1.*
FROM QuizDetails t1
LEFT JOIN QuizAttempts t2 ON t2.QuizId = t1.ParentQuizId
WHERE t2.QuizId IS NULL and 
DATEADD(year, 2,t1.CreatedOn) < getdate()order by 1 desc
";

            UOWObj.QuizAttemptsRepository.DeleteRange(query);
            UOWObj.Save();
            return query;
        }
    }
}