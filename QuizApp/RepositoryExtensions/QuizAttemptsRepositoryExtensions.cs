using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizAttemptsRepositoryExtensions
    {

        public static IEnumerable<QuizAttempts> GetQuizAttemptsBasicDetailsByCode(this GenericRepository<QuizAttempts> repository, string  quizCode)
        {
            return repository.Get(v => v.Code == quizCode);
        }

        public static IEnumerable<QuizAttempts> GetQuizAttemptsById(this GenericRepository<QuizAttempts> repository, int quizAttemptId)
        {
            return repository.Get(v => v.Id == quizAttemptId);
        }

    }
}