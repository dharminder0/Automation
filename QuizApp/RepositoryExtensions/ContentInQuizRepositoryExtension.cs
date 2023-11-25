using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class ContentInQuizRepositoryExtension
    {

        public static IEnumerable<ContentsInQuiz> GetContentInQuizRepositoryExtension(this GenericRepository<ContentsInQuiz> repository, int quizDetailId)
        {
            return repository.Get(v => v.QuizId == quizDetailId);
        }
        public static IEnumerable<ContentsInQuiz> GetContentInQuizById(this GenericRepository<ContentsInQuiz> repository, int status, int id)
        {
            return repository.Get(r => r.Status == status && r.Id == id );
        }
    }
}