using QuizApp.Services.Model;
using Swashbuckle.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Request
{
    public class CronRequest
    {
        public string ToEmail { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<FileAttachment> Attachments { get; set; }
        public string ClientCode { get; set; }

        public Cron MapRequestToEntity(CronRequest cronRequestObj)
        {
            Cron cron = new Cron();

            cron.ToEmail = cronRequestObj.ToEmail;
            cron.Subject = cronRequestObj.Subject;
            cron.Body = cronRequestObj.Body;
            cron.Attachments = cronRequestObj.Attachments;
            cron.ClientCode = cronRequestObj.ClientCode;

            return cron;
        }
    }
    public class CronRequestExample : IExamplesProvider
    {
        public object GetExamples()
        {
            var attachment = new List<FileAttachment>();
            attachment.Add(new FileAttachment() {FileLink = string.Empty,FileName = string.Empty });

            return new CronRequest
            {
                ToEmail = string.Empty,
                Subject = string.Empty,
                Body = string.Empty,
                Attachments = attachment,
                ClientCode = string.Empty
            };
        }
    }
}