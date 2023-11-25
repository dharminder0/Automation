using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class BadgesInQuizRepositoryExtension
    {

        public static IEnumerable<BadgesInQuiz> GetBadgesInQuizById(this GenericRepository<BadgesInQuiz> repository, int status, int id)
        {
            return repository.Get(a => a.Status == status && a.Id == id);
        }
    }
}
