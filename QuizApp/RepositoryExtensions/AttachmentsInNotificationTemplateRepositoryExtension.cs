using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class AttachmentsInNotificationTemplateRepositoryExtension
    {
        public static IEnumerable<AttachmentsInNotificationTemplate> GetAttachmentsInNotificationTemplateByTemplateId(this GenericRepository<AttachmentsInNotificationTemplate> repository, int notificationTemplateId)
        {
            return repository.Get(v => v.NotificationTemplateId == notificationTemplateId);
        }
     
    }
}
