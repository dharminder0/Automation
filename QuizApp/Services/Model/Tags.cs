using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class AutomationTagsDetails : Base
    {
        public int categoryId { get; set; }
        public string categoryName { get; set; }
        public int tagId { get; set; }
        public string tagName { get; set; }
        public string tagCode { get; set; }
    }
}