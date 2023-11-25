using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuizApp.Request
{
    public class QuizListRequest
    {
        public List<string> OfficeIdList { get; set; }
        public bool IncludeSharedWithMe { get; set; }
        public long OffsetValue { get; set; }
        public string SearchTxt { get; set; } = "";
        public string QuizTypeId { get; set; }
        public bool IsDataforGlobalOfficeAdmin { get; set; }
        public string QuizId { get; set; } 
        public bool? IsFavorite { get; set; }
        public bool? IsPublished { get; set; }
        public int? QuizTagId { get; set; }
        public int OrderBy { get; set; }
        public int PageNo { get; set; }
        public int PageSize { get; set; }
        public int? UsageType { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }
    }
    public class AutomationListRequest
    {
        public bool IncludeSharedWithMe { get; set; }
        public long OffsetValue { get; set; }
        public string SearchTxt { get; set; }
        public int? OrderBy { get; set; }
        public string QuizTypeId { get; set; }
        public bool IsDataforGlobalOfficeAdmin { get; set; }
        public string OfficeIdList { get; set; }
        public int? PageNo { get; set; }
        public int? PageSize { get; set; }
        public string QuizId { get; set; }
        public bool? IsFavorite { get; set; }
        public string MustIncludeQuizId { get; set; }
        public int? UsageType { get; set; }
        public int? QuizTagId { get; set; }
        public bool? IsWhatsAppChatBotOldVersion { get; set; }
    }
}