using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static QuizApp.Response.QuizTemplateResponse;

namespace QuizApp.Services.Model
{
    public class WhatsappMessage
    {
        public string ClientCode { get; set; }
        public string ContactPhone { get; set; }
        public int HsmTemplateId { get; set; }
        public string LanguageCode { get; set; }
        public List<TemplateParameter> TemplateParameters { get; set; }
        public string FollowUpMessage { get; set; }
        public int ModuleWorkPackageId { get; set; }
        public string ModuleName { get; set; }
        public string EventType { get; set; }
        public string  ObjectId { get; set; }
        public string ContactId { get; set; }
        public string UniqueCode { get; set; }
        public List<ButtonParam> ButtonParams { get; set; }
        public List<HeaderParameter> HeaderParams { get; set; }
        public class TemplateParameter
        {
            public string paraname { get; set; }
            public int position { get; set; }
            public string value { get; set; }
        }

        public class HeaderParameter {
            public string paraname { get; set; }
            public int position { get; set; }
            public string value { get; set; }
        }

        public class ButtonParam
        {
            public int ButtonIndex { get; set; }
            public List<TemplateParameter> Params { get; set; }
        }

    }
}