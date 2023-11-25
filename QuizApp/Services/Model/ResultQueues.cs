using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Services.Model
{
    public class ResultQueues
    {
        public int Id { get; set; }
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string ToPhone { get; set; }
        public string SMSText { get; set; }
        public DateTime? SentOn { get; set; }
        public int Type { get; set; }
        public bool Sent { get; set; }
        public List<FileAttachment> Attachments { get; set; }
        public CompanyModel Company { get; set; }
    }
}