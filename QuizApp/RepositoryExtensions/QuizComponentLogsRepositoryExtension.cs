using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizComponentLogsRepositoryExtension
    {
        public static IEnumerable<QuizComponentLogs> GetQuizComponentLogsByQuizId(this GenericRepository<QuizComponentLogs> repository, int quizDetailId, int status)
        {
            return repository.Get(r => r.QuizId == quizDetailId && r.ObjectTypeId == status);
        }
    }
}




