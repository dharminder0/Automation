using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizObjectStatsRepositoryExtension
    {
        public static IEnumerable<QuizObjectStats> GetQuizObjectStatsByQuizAttemptId(this GenericRepository<QuizObjectStats> repository, int quizAttemptId)
        {
            return repository.Get(r => r.QuizAttemptId == quizAttemptId);
        }
    }
}
