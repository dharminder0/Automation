using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request
{
    public class NotificationTemplateQuizListRequestModel
    {
        public int NotificationType { get; set; }
        public string OfficeId { get; set; } = "";
        public bool IncludeSharedWithMe { get; set; } = true;
        public int PageNo { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string SearchTxt { get; set; } = "";
        public int OrderBy { get; set; } = 0;
        public string QuizId { get; set; } = "";
        public string QuizTypeId { get; set; } = "";
        public bool? IsFavorite { get; set; }
        public int? QuizTagId { get; set; }
        public int? UsageType { get; set; }

    }



    public class NotificationTemplateInActiveRequestModel : NotificationTemplateQuizListRequestModel
    {
        public int TemplateId { get; set; }
    }
}