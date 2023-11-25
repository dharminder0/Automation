using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizRepositoryExtension
    {
        public static IEnumerable<Quiz> GetQuizById(this GenericRepository<Quiz> repository, int id)
        {
            return repository.Get(r => r.Id == id);
        }
    }
}