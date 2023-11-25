using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace QuizApp.Response
{
    public class QuizTemplateResponse : IResponse
    {
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
        public List<Template> Templates { get; set; }
        public class Template
        {
            public int Id { get; set; }
            public string QuizTitle { get; set; }
            public string QuizDescription { get; set; }
            public string CoverTitle { get; set; }
            public int QuizType { get; set; }
            public string CoverImage { get; set; }
            public string PublicIdForQuizCover { get; set; }
            public int TotalQuestion { get; set; }
            public string PublishedCode { get; set; }
        }
        public class Button
        {
            public int ButtonIndex { get; set; }
            public List<Param> Params { get; set; }
        }

        public class ButtonParam
        {
            public string ModuleCode { get; set; }
            public List<Button> Buttons { get; set; }
        }

        public class CustomComponent
        {
            public string Type { get; set; }
            public List<Item> Items { get; set; }
        }

        public class EnabledTemplateElements
        {
            public bool Header { get; set; }
            public bool Footer { get; set; }
            public bool Buttons { get; set; }
            public bool Body { get; set; }
        }

        public class Item
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public string Type { get; set; }
            public string SubType { get; set; }
            public string Url { get; set; }
            public object Phone_number { get; set; }
            public object MappedFields { get; set; }
            public string Example { get; set; }
        }

        public class Param
        {
            public string ModuleCode { get; set; }
            public List<Param> Params { get; set; }
            public string Paraname { get; set; }
            public int Position { get; set; }
            public string Value { get; set; }
        }

        public class WhatsappTemplate
        {
            public int Id { get; set; }
            public string TemplateName { get; set; }
            public string DisplayName { get; set; }
            public string TemplateNamespace { get; set; }
            public List<TemplateBody> TemplateBody { get; set; }
            public string Provider { get; set; }
            public List<Param> Params { get; set; }
            public List<WhatsappParam> Paramms { get; set; }
            public List<ButtonParam> ButtonParams { get; set; }
            public List<HeaderParam> HeaderParams { get; set; }
            public int SortOrder { get; set; }
            public bool IsActive { get; set; }
            public string ClientCode { get; set; }
            public List<string> TemplateTypes { get; set; }
            public string CategoryId { get; set; }
            public string CategoryName { get; set; }
        }
        public class HeaderParam {
            public string moduleCode { get; set; }
            public List<HeaderPara> Params { get; set; }
        }
        public class HeaderPara {
            public string Paraname { get; set; }
            public int Position { get; set; }
            public string Value { get; set; }
        }

        public class TemplateBody
        {
            public string LangCode { get; set; }
            public string TempBody { get; set; }
            public List<CustomComponent> CustomComponents { get; set; }
            public string Status { get; set; }
            public object AllowedAnswers { get; set; }
            public object HeaderType { get; set; }
            public object FooterText { get; set; }
            public object HeaderText { get; set; }
            public bool IsEnabledDynamicMedia { get; set; }
            public MediaVariables MediaVariable { get; set; }
            public object BodyParamsExamples { get; set; }
            public object HeaderExample { get; set; }
            public string HeaderMediaPublicId { get; set; }
            public object DefaultHeaderMediaUrl { get; set; }
            public string TemplateStructureType { get; set; }
            public EnabledTemplateElements EnabledTemplateElements { get; set; }
        }

        public class MediaVariables {
            public string MediaOwner { get; set; }
            public string ProfileMedia { get; set; }
            public object MediaUrlValue { get; set; }
        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizTemplateResponse response = new QuizTemplateResponse();

            var templateObj = (QuizTemplate)EntityObj;

            response.CurrentPageIndex = templateObj.CurrentPageIndex;
            response.TotalRecords = templateObj.TotalRecords;

            response.Templates = new List<Template>();

            foreach (var obj in templateObj.Templates)
            {
                var template = new Template();
                template.Id = obj.Id;
                template.QuizTitle = obj.QuizTitle == null ? string.Empty : obj.QuizTitle;
                template.QuizDescription = obj.QuizDescription == null ? string.Empty : obj.QuizDescription;
                template.CoverTitle = obj.CoverTitle == null ? string.Empty : obj.CoverTitle;
                template.QuizType = (int)obj.QuizType;
                template.CoverImage = obj.CoverImage;
                template.PublicIdForQuizCover = obj.PublicIdForQuizCover;
                template.TotalQuestion = obj.TotalQuestion;
                template.PublishedCode = obj.PublishedCode;
                response.Templates.Add(template);
            }

            return response;
        }
    }


    public class QuizDashboardTemplateResponse : IResponse
    {
        public int CurrentPageIndex { get; set; }
        public int TotalRecords { get; set; }
        public List<DashboardTemplate> Templates { get; set; }
        public class DashboardTemplate
        {
            public int Id { get; set; }
            public string QuizTitle { get; set; }
            public bool IsPublished { get; set; }
            public DateTime CreatedOn { get; set; }
            public string PublishedCode { get; set; }
            public int QuizTypeId { get; set; }
            public int Status { get; set; }
            public int CategoryId { get; set; }
        }

        public IResponse MapEntityToResponse(Base EntityObj)
        {
            QuizDashboardTemplateResponse response = new QuizDashboardTemplateResponse();

            var templateObj = (QuizTemplate)EntityObj;

            response.CurrentPageIndex = templateObj.CurrentPageIndex;
            response.TotalRecords = templateObj.TotalRecords;

            response.Templates = new List<DashboardTemplate>();

            foreach (var obj in templateObj.Templates)
            {
                var template = new DashboardTemplate();
                template.Id = obj.Id;
                template.QuizTitle = obj.QuizTitle == null ? string.Empty : obj.QuizTitle;
                template.PublishedCode = obj.PublishedCode;
                template.IsPublished = obj.IsPublished;
                template.CreatedOn = obj.CreatedOn;
                template.QuizTypeId = (int)obj.QuizType;
                template.Status = (int)obj.Status;
                template.CategoryId = obj.CategoryId;

                response.Templates.Add(template);
            }

            return response;
        }
    }
}