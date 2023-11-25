using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizStatsRepositoryExtension
    {
       

        public static IEnumerable<QuizStats> GetQuizStatsQuizAttemptId(this GenericRepository<QuizStats> repository, int quizAttemptId)
        {
            return repository.Get(v => v.QuizAttemptId == quizAttemptId);
        }

        public static IEnumerable<QuizStats> GetUncompleteQuizStats(this GenericRepository<QuizStats> repository, DateTime startDate, DateTime endDate) {
          
            return repository.Get(v => v.StartedOn < startDate && v.StartedOn > endDate && v.ResultId == null && v.CompletedOn == null && v.AttemptStatus != 4).OrderByDescending(r=>r.Id).Take(10);
        }
    }
}