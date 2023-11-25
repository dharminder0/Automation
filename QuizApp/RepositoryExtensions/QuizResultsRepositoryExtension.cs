using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizResultsRepositoryExtension
    {

        public static IEnumerable<QuizResults> GetQuizResultsRepositoryExtension(this GenericRepository<QuizResults> repository, int quizId)
        {
            return repository.Get(v => v.QuizId == quizId);
        }


        public static IEnumerable<QuizResults> GetQuizResultsById(this GenericRepository<QuizResults> repository, int status, int id)
        {
            return repository.Get(a => a.Status ==  status && a.Id == id);
        }
    }
}