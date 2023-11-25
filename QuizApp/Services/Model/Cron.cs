using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Services.Model
{
    public class Cron
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<FileAttachment> Attachments { get; set; }
        public string ClientCode { get; set; }
    }
}