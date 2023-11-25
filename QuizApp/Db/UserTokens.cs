
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class UserTokens
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int BusinessUserId { get; set; }
        [StringLength(1000)]
        public string OWCToken { get; set; }
        public bool CreateAcademyCourse { get; set; }
        public bool CreateTechnicalRecruiterCourse { get; set; }
        public bool CreateTemplate { get; set; }       

        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public virtual Company Company { get; set; }

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

        public virtual ICollection<FavoriteQuizByUser> FavoriteQuizByUser { get; set; }
    }
}