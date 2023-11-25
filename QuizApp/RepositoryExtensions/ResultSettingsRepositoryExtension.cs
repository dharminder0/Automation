using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class ResultSettingsRepositoryExtension
    {

        public static IEnumerable<ResultSettings> GetResultSettingsRepositoryExtension(this GenericRepository<ResultSettings> repository, int quizDetailId)
        {
            return repository.Get(v => v.QuizId == quizDetailId);
        }

    }
}