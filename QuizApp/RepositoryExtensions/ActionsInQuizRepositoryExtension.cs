using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public  static class ActionsInQuizRepositoryExtension
    {
        public static IEnumerable<ActionsInQuiz> GetActionInQuiz(this GenericRepository<ActionsInQuiz> repository, int id)
        {
            return repository.Get(r => r.Id == id && r.Status == (int)StatusEnum.Active);
        }
    }
}