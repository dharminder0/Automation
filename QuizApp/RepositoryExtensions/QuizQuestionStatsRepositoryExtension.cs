using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizQuestionStatsRepositoryExtension
    {
        public static IEnumerable<QuizQuestionStats> GetQuizQuestionStatsByQuizAttemptId(this GenericRepository<QuizQuestionStats> repository, int quizAttemptId)
        {
            return repository.Get(r => r.QuizAttemptId == quizAttemptId);
        }
        public static IEnumerable<QuizQuestionStats> GetQuizQuestionStatsByQuestionId(this GenericRepository<QuizQuestionStats> repository, int quizAttemptId,int questionId)
        {
            return repository.Get(v => v.QuizAttemptId == quizAttemptId && v.QuestionId == questionId);
        }


    }
}
