using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class Course : Base
    {
        public int Id { get; set; }
        public string AccessibleOfficeId { get; set; }
        public DateTime? LastAttempt { get; set; }
        public DateTime CreatedOn { get; set; }
        public int NumberOfAttempted { get; set; }
        public QuizAttachment QuizAttachment { get; set; }
        public bool IsStarted { get; set; }
        public string PublishedCode { get; set; }
        public string Title { get; set; }
        public string CoverImage { get; set; }
        public string PublicIdForQuizCover { get; set; }
        public int Type { get; set; }
        public QuizBadge QuizBadge { get; set; }
    }
    public class Dashboard : Base
    {
        public int ModuleId { get; set; }
        public string ModuleName { get; set; }
        public int NumberOfElearning { get; set; }
        public int NumberOfAttempted { get; set; }
    }
}