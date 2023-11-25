using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Response {
    public class WhatsappTemplateV3 {

        public class WhatsAppParameter {
            public string paraname { get; set; }
            public int position { get; set; }
            public string value { get; set; }
        }


        public class Button {
            public int ButtonIndex { get; set; }
            public List<WhatsAppParameter> Params { get; set; }
        }



        public class ButtonParam {
            public string ModuleCode { get; set; }
            public List<Button> Buttons { get; set; }
        }

        public class CustomComponent {
            public string Type { get; set; }
            public List<Item> Items { get; set; }
        }
        public class ButtonList {
            public int Id { get; set; }
            public string Text { get; set; }
            public string Type { get; set; }
            public string SubType { get; set; }
            public string Url { get; set; }
            public object Phone_number { get; set; }
            public object MappedFields { get; set; }
            public string Example { get; set; }
        }

        public class Item {
            public int Id { get; set; }
            public string Text { get; set; }
            public string Type { get; set; }
            public string SubType { get; set; }
            public string Url { get; set; }
            public object Phone_number { get; set; }
            public object MappedFields { get; set; }
            public string Example { get; set; }
        }

        public class ParamItem {
            public string ModuleCode { get; set; }
            public List<WhatsAppParameter> Params { get; set; }
        }








        public class TemplateBody {
            public string LangCode { get; set; }
            public string TempBody { get; set; }
            public List<CustomComponent> CustomComponents { get; set; }
            public List<ButtonList> Buttons { get; set; }
            public string Status { get; set; }
            public object AllowedAnswers { get; set; }
            public object HeaderType { get; set; }
            public object FooterText { get; set; }
            public object HeaderText { get; set; }
            public object BodyParamsExamples { get; set; }
            public object HeaderExample { get; set; }
            public object DefaultHeaderMediaUrl { get; set; }
            public string TemplateStructureType { get; set; }
            public EnabledTemplateElements EnabledTemplateElements { get; set; }
        }
        public class EnabledTemplateElements {
            public bool Header { get; set; }
            public bool Footer { get; set; }
            public bool Buttons { get; set; }
            public bool Body { get; set; }
        }

        public class SendWhatsappMessageV3 {
            public string ClientCode { get; set; }
            public string ContactPhone { get; set; }
            public int HsmTemplateId { get; set; }
            public string LanguageCode { get; set; }
            public TemplateBody TemplateBody { get; set; }
            public List<WhatsAppParameter> TemplateParameters { get; set; }
            public string FollowUpMessage { get; set; }
            public int ModuleWorkPackageId { get; set; }
            public string ModuleName { get; set; }
            public List<Button> ButtonParams { get; set; }
            public List<WhatsAppParameter> HeaderParams { get; set; }






        }

        public class GetWhatsappTemplateV3 {
            public int Id { get; set; }
            public string TemplateName { get; set; }
            public string DisplayName { get; set; }
            public string TemplateNamespace { get; set; }
            public List<TemplateBody> TemplateBody { get; set; }
            public string Provider { get; set; }
            public List<ParamItem> Params { get; set; }
            public List<ButtonParam> ButtonParams { get; set; }
            public List<ParamItem> HeaderParams { get; set; }
            public int SortOrder { get; set; }
            public bool IsActive { get; set; }
            public string ClientCode { get; set; }
            public List<string> TemplateTypes { get; set; }
            public string CategoryId { get; set; }
            public string CategoryName { get; set; }
            public object TemplateLanguage { get; set; }
            public string AudienceType { get; set; }
        }
    }
}