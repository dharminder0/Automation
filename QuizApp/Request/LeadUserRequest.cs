using Core.Common.Extensions;
using QuizApp.Response;
using QuizApp.Services.Model;
using Swashbuckle.Examples;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;

namespace QuizApp.Request
{
    public class LeadUserRequest
    {
        public string PublishedCode { get; set; }
        public string INFS_TAG { get; set; }
        public string Voornaam { get; set; }
        public string Achternaam { get; set; }
        public string Email { get; set; }
        public string CombinedPhoneNr { get; set; }
        public string CompanyCode { get; set; }       
        public string SourceType { get; set; }
        public string SourceId { get; set; }
        public string Utm_Source { get; set; }
        public string Utm_Medium { get; set; }
        public string Utm_Content { get; set; }
        public string Utm_Campaign { get; set; }
        public string Utm_Term { get; set; }
        public string Utm_Channel { get; set; }
        public string GAClientId { get; set; }
        public string Utm_id { get; set; }
        public string Web_id { get; set; }
        public string Origin { get; set; }
        public string OriginId { get; set; }
        public ResumeObj Resume { get; set; }


        public Dictionary<string, object> MapRequestToHubEntity(LeadUserRequest LeadObj, string CompanyName,string companyCode, int QuizId)
        {
            Lead obj = new Lead();

            Dictionary<string, object> dynamicFields = new Dictionary<string, object> {
                 {"FirstName", LeadObj.Voornaam},
                 {"LastName", LeadObj.Achternaam},
                 {"Email", LeadObj.Email},
                 {"Phone", LeadObj.CombinedPhoneNr},
                 {"ClientCode", LeadObj.CompanyCode},
                 {"GASource", LeadObj.Utm_Source},
                 {"GAMedium", LeadObj.Utm_Medium},
                 {"GAChannelGrouping", LeadObj.Utm_Channel},
                 {"PubId", LeadObj.Utm_id},
                 {"GATerm", LeadObj.Utm_Term},
                 {"SourceId", LeadObj.SourceId},
                 {"OriginId",(string.IsNullOrWhiteSpace(LeadObj.Origin)) ? "Automation" : LeadObj.Origin },
                 {"Origin",(string.IsNullOrWhiteSpace(LeadObj.Origin)) ? "Automation" : LeadObj.Origin },
                 {"WebId", LeadObj.Web_id},
                 {"GAClientId", LeadObj.GAClientId},
                 {"GACampaign", LeadObj.Utm_Campaign},
                 {"ClientName",CompanyName},
                 
               {"GAId", LeadObj.Utm_id},
            };

            if (LeadObj.Resume != null && LeadObj.Resume.File != null) {
                if (!string.IsNullOrWhiteSpace(LeadObj.Resume.File.FileName)) {
                    dynamicFields.Add("CVFileName", LeadObj.Resume.File.FileName);
                }
                if (LeadObj.Resume.File.Data != null) {
                    dynamicFields.Add("CVFileData", Convert.FromBase64String(LeadObj.Resume.File.Data));
                }
            }

            return dynamicFields;
        }
        public Lead MapRequestToEntity(LeadUserRequest LeadObj, string CompanyName, string companyCode, int QuizId) {
            Lead obj = new Lead();

            obj.INFS_TAG = new string[] { LeadObj.INFS_TAG };
            obj.Voornaam = new string[] { LeadObj.Voornaam };
            obj.Achternaam = new string[] { LeadObj.Achternaam };
            obj.Email = new string[] { LeadObj.Email };
            obj.CombinedPhoneNr = new string[] { LeadObj.CombinedPhoneNr };
            obj.utm_content = new string[] { LeadObj.Utm_Content };
            obj.utm_medium = new string[] { LeadObj.Utm_Medium };
            obj.utm_source = new string[] { LeadObj.Utm_Source };
            obj.utm_campaign = new string[] { LeadObj.Utm_Campaign };
            obj.utm_term = new string[] { LeadObj.Utm_Term };
            obj.utm_channel = new string[] { LeadObj.Utm_Channel };
            obj.sourceType = new string[] { LeadObj.SourceType };
            obj.sourceId = new string[] { LeadObj.SourceId };
            obj.clientName = new string[] { CompanyName };
            obj.GAClientId = new string[] { LeadObj.GAClientId };
            //obj.Origin = new string[] { "Automation" };
            //obj.OriginId = new string[] { QuizId.ToString() };
            obj.Origin = new string[] { (string.IsNullOrWhiteSpace(LeadObj.Origin)) ? "Automation" : LeadObj.Origin };
            obj.OriginId = new string[] { (string.IsNullOrWhiteSpace(LeadObj.OriginId)) ? QuizId.ToString() : LeadObj.OriginId };
            obj.utm_id = new string[] { LeadObj.Utm_id };
            obj.web_id = new string[] { LeadObj.Web_id };
            obj.Resume = LeadObj.Resume;
            obj.ClientCode = new string[] { LeadObj.CompanyCode };

            obj.UserToken = (GlobalSettings.leadUserToken).ToString();

            return obj;
        }
        public CompanyModel MapCompanyResponseToEntity(OWCCompanyResponse OWCCompanyResponse)
        {
            return new CompanyModel()
            {
                Id = OWCCompanyResponse.id,
                AlternateClientCodes = OWCCompanyResponse.alternateClientCodes,
                ClientCode = OWCCompanyResponse.clientCode,
                CompanyName = OWCCompanyResponse.companyName,
                CompanyWebsiteUrl = OWCCompanyResponse.companyWebsiteUrl,
                JobRocketApiAuthorizationBearer = OWCCompanyResponse.jobRocketApiAuthorizationBearer,
                JobRocketApiUrl = OWCCompanyResponse.jobRocketApiUrl,
                JobRocketClientUrl = OWCCompanyResponse.jobRocketClientUrl,
                LeadDashboardApiAuthorizationBearer = OWCCompanyResponse.leadDashboardApiAuthorizationBearer,
                LeadDashboardApiUrl = OWCCompanyResponse.leadDashboardApiUrl,
                LeadDashboardClientUrl = OWCCompanyResponse.leadDashboardClientUrl,
                LogoUrl = OWCCompanyResponse.logoUrl,
                PrimaryBrandingColor = OWCCompanyResponse.primaryBrandingColor,
                SecondaryBrandingColor = OWCCompanyResponse.secondaryBrandingColor
            };
        }

        public class LeadUserRequestExample : IExamplesProvider
        {
            public object GetExamples()
            {
                return new LeadUserRequest
                {
                    PublishedCode = string.Empty,
                    INFS_TAG = string.Empty,
                    Voornaam = string.Empty,
                    Achternaam = string.Empty,
                    Email = string.Empty,
                    CombinedPhoneNr = string.Empty,
                    CompanyCode = string.Empty,
                    SourceType = string.Empty,
                    SourceId = string.Empty,
                    Utm_Source = string.Empty,
                    Utm_Medium = string.Empty,
                    Utm_Content = string.Empty,
                    Utm_Campaign = string.Empty,
                    Utm_Term = string.Empty,
                    Utm_Channel = string.Empty,
                    GAClientId = string.Empty,
                    Resume = new ResumeObj()
                    {
                        File = new FileObj()
                        {
                            FileName = string.Empty,
                            ContentType = string.Empty,
                            Data = string.Empty
                        }
                    }
                };
            }
        }
    }
}