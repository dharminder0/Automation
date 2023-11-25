using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class AttemptQuizLogLocal : Base
    {
        public int Id { get; set; }
        public int QuizId { get; set; }
        public int LeadId { get; set; }
        public RequestJson RequestJson { get; set; }
        public QuizAnswerSubmit ResponseJson { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class RequestJson : Base
    {
        public List<TextAnswer> TextAnswerList { get; set; }
        public string QuizCode { get; set; }
        public string Mode { get; set; }
        public string Type { get; set; }
        public int QuestionId { get; set; }
        public string AnswerIdList { get; set; }
        public int BusinessUserId { get; set; }
        public int UserTypeId { get; set; }
        public int? QuestionType { get; set; }
        public int? UsageType { get; set; }

    }
}