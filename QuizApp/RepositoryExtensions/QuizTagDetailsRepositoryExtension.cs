using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizTagDetailsRepositoryExtension
    {
        public static IEnumerable<QuizTagDetails> GetQuizTagDetailsByQuizId(this GenericRepository<QuizTagDetails> repository, int quizId)
        {
            return repository.Get(r => r.QuizId == quizId);
        }
    }
}