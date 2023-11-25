using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class NotificationTemplatesInQuizRepositoryExtension
    {
        public static IEnumerable<NotificationTemplatesInQuiz> GetNotificationTemplatesInQuizByQuizId(this GenericRepository<NotificationTemplatesInQuiz> repository, int QuizDetailid)
        {
            return repository.Get(r => r.QuizId == QuizDetailid);
        }
    }
}