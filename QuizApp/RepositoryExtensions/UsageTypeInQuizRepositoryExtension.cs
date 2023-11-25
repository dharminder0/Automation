using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class UsageTypeInQuizRepositoryExtension
    {

        public static IEnumerable<UsageTypeInQuiz> GetUsageTypeInQuizRepositoryExtension(this GenericRepository<UsageTypeInQuiz> repository, int quizId)
        {
            return repository.Get(v => v.QuizId == quizId);
        }

    }
}