using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class MediaVariablesDetailsRepositoryExtension
    {
        public static IEnumerable<MediaVariablesDetails> GetMediaVariablesDetailsByQuizId(this GenericRepository<MediaVariablesDetails> repository, int quizDetailId)
        {
            return repository.Get(r => r.QuizId == quizDetailId);
        }
    }
}