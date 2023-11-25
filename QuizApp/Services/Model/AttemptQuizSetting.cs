using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class AttemptQuizSetting
    {
            public string SourceType { get; set; }
            public string SourceId { get; set; }
            public string SourceName { get; set; }
            public string PrivacyLink { get; set; }
            public PrivacyDto PrivacyJson { get; set; }
            public string ConfigurationType { get; set; }
            public string ConfigurationId { get; set; }
            public string PrimaryBrandingColor { get; set; }
            public string SecondaryBrandingColor { get; set; }
            public string LeadFormTitle { get; set; }
            public string GtmCode { get; set; }
            public string FavoriteIconUrl { get; set; }
            public string OfficeId { get; set; }
            public string CompanyCode { get; set; }
            public List<Tags> Tag { get; set; }
            public QuizBrandingAndStyleModel QuizBrandingAndStyle { get; set; }
            public QuizCover QuizCoverDetails { get; set; }            
            public bool LoadQuizDetails { get; set; }
            public List<int> UsageType { get; set; }


    }
}