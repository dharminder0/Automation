using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Services.Model
{
    public class CompanyModel
    {
        public int Id { get; set; }
        public string ClientCode { get; set; }
        public string JobRocketApiUrl { get; set; }
        public string JobRocketApiAuthorizationBearer { get; set; }
        public string JobRocketClientUrl { get; set; }
        public string LeadDashboardApiUrl { get; set; }
        public string LeadDashboardApiAuthorizationBearer { get; set; }
        public string LeadDashboardClientUrl { get; set; }
        public string PrimaryBrandingColor { get; set; }
        public string SecondaryBrandingColor { get; set; }
        public string TertiaryColor { get; set; }
        public string LogoUrl { get; set; }
        public string CompanyName { get; set; }
        public string AlternateClientCodes { get; set; }
        public string CompanyWebsiteUrl { get; set; }
        public bool CreateAcademyCourseEnabled { get; set; }
        public bool CreateTechnicalRecruiterCourseEnabled { get; set; }
        public bool CreateTemplateEnabled { get; set; }
        public bool BadgesEnabled { get; set; }
        public List<GtmSetting> gtmSettings { get; set; }
    }
}