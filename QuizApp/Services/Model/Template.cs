using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class QuizTemplate : Base
    {
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
        public List<Template> Templates { get; set; }
    }

    public class Template
    {
        public int Id { get; set; }
        public string QuizTitle { get; set; }
        public string QuizDescription { get; set; }
        public string CoverTitle { get; set; }
        public QuizTypeEnum QuizType { get; set; }
        public string CoverImage { get; set; }
        public string  PublicIdForQuizCover { get; set; }
        public int TotalQuestion { get; set; }
        public bool IsPublished { get; set; }
        public DateTime CreatedOn { get; set; }
        public string PublishedCode { get; set; }
        public StatusEnum Status { get; set; }
        public int CategoryId { get; set; }
    }
}