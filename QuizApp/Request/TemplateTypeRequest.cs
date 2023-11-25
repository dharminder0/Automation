using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Request
{
    public class TemplateTypeRequest
    {
        public class TemplateTypeRequestModel
        {
            public int NotificationType { get; set; }
            public string OfficeId { get; set; } = "";
            public bool IncludeSharedWithMe { get; set; } = true;
            public int PageNo { get; set; } = 1;
            public int PageSize { get; set; } = 20;

        }
    }
}

