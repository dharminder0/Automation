using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class NotificationTemplateType : Base
    {
        public List<NotificationTemplateModel> NotificationTemplateList { get; set; }
        public List<LocalQuiz> InactiveQuizList { get; set; }
    }

    public class NotificationTemplateTypeV1 : Base
    {
        public List<NotificationTemplateModel> NotificationTemplateList { get; set; }
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
    }
}