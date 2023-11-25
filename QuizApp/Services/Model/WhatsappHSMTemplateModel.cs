using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class WhatsappHSMTemplateModel
    {
        public int id { get; set; }
        public string templateName { get; set; }
        public string displayName { get; set; }
        public string templateNamespace { get; set; }
        public string templateLanguage { get; set; }
        public string templateBody { get; set; }
        public string provider { get; set; }
        public Customcomponent[] customComponents { get; set; }
        public Param[] _params { get; set; }
        public object buttonParams { get; set; }
        public int sortOrder { get; set; }
        public bool isActive { get; set; }
        public string clientCode { get; set; }
        public object allowedAnswers { get; set; }
        public string[] templateTypes { get; set; }
        public bool isConsentTemplate { get; set; }
        public object categoryId { get; set; }
        public object categoryName { get; set; }
        public string status { get; set; }

        public class Customcomponent
        {
            public string type { get; set; }
            public Item[] items { get; set; }
        }

        public class Item
        {
            public int id { get; set; }
            public string text { get; set; }
            public object type { get; set; }
            public object url { get; set; }
            public object phone_number { get; set; }
            public Mappedfield[] mappedFields { get; set; }
            public object example { get; set; }
        }

        public class Mappedfield
        {
            public string objectName { get; set; }
            public string fieldName { get; set; }
            public string value { get; set; }
        }

        public class Param
        {
            public string paraname { get; set; }
            public int position { get; set; }
            public object value { get; set; }
        }
    }
}