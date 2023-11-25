using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Model
{
    public class BusinessUser : Base
    {
        public int Id { get; set; }
        public int BusinessUserId { get; set; }
        public string OWCToken { get; set; }
        public bool CreateAcademyCourse { get; set; }
        public bool CreateTechnicalRecruiterCourse { get; set; }
        public bool CreateTemplate { get; set; }
        public List<OfficeModel> OfficeList { get; set; }
        public CompanyModel CompanyInfo { get; set; }
        public bool IsGlobalOfficeAdmin { get; set; }
        public bool IsAutomationTechnicalRecruiterCoursePermission { get; set; }
        public bool IsAppointmentTechnicalRecruiterCoursePermission { get; set; }
        public bool IsELearningTechnicalRecruiterCoursePermission { get; set; }
        public bool IsCanvasTechnicalRecruiterCoursePermission { get; set; }
        public bool IsVacanciesTechnicalRecruiterCoursePermission { get; set; }
        public bool IsContactsTechnicalRecruiterCoursePermission { get; set; }
        public bool IsReviewTechnicalRecruiterCoursePermission { get; set; }
        public bool IsReportingTechnicalRecruiterCoursePermission { get; set; }
        public bool IsCampaignsTechnicalRecruiterCoursePermission { get; set; }
        public bool IsCreateStandardAutomationPermission { get; set; }
        public bool IsManageElearningPermission { get; set; }
        public bool IsWebChatbotPermission { get; set; }
    }
}