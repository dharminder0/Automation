using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class Lead
    {
        public string[] utm_source { get; set; }
        public string[] utm_medium { get; set; }
        public string[] utm_content { get; set; }
        public string[] utm_campaign { get; set; }
        public string[] utm_term { get; set; }
        public string[] utm_channel { get; set; }
        public string[] INFS_TAG { get; set; }
        public string[] Voornaam { get; set; }
        public string[] Achternaam { get; set; }
        public string[] Email { get; set; }
        public string[] CombinedPhoneNr { get; set; }
        public string[] sourceType { get; set; }
        public string[] sourceId { get; set; }
        public string[] clientName { get; set; }
        public string[] GAClientId { get; set; }
        public string UserToken { get; set; }
        public string[] Origin { get; set; }
        public string[] OriginId { get; set; }
        public string[] ClientCode { get; set; }
        public string[] utm_id { get; set; }
        public string[] web_id { get; set; }
        public ResumeObj Resume { get; set; }
    }

    public class ResumeObj
    {
        public FileObj File { get; set; }
    }

    public class FileObj
    {
        public string FileName { get; set; }
        public string ContentType { get; set; }
        public string Data { get; set; }
    }

    public class LeadTags
    {
        public string LeadId { get; set; }
        public int[] TagsIds { get; set; }
    }

    public class LeadQuizStatus
    {
        public string AutomationTitle { get; set; }
        public string CreatedDate { get; set; }
        public string StartedDate { get; set; }
        public string AttemptDate { get; set; }
        public string SourceId { get; set; }
        public string ContactId { get; set; }
        public string ClientCode { get; set; }
        public int QuizId { get; set; }
        public string QuizStatus { get; set; }
        public string RequestId { get; set; }
        public string ConfigurationId { get; set; }
        public int QuizType { get; set; }
        public List<Result> Results { get; set; }
        public List<Fields> FieldsToUpdate { get; set; }
    }

    public class Result
    {
        public int ResultId { get; set; }
        public int ParentResultId { get; set; }
        public string ResultTitle { get; set; }
    }

    public class Fields
    {
        public string ObjectName { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
    }

    public class UnknownLeadActions
    {
        public string ContactId { get; set; }
        public string ClientCode { get; set; }
        public string ConfigurationId { get; set; }
        public List<Result> Results { get; set; }
    }
}