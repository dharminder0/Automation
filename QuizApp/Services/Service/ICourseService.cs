using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service
{
    public interface ICourseService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        List<Model.Course> GetDashboardData(long OffsetValue, bool IncludeSharedWithMe, string BusinessUserEmail, string OfficeIds, int BusinessUserId, int CompanyId, bool IsJobRockAcademy);
        List<Model.Course> GetTechnicalRecruiterData(long OffsetValue, string BusinessUserEmail, int Module, int BusinessUserId, BusinessUser UserInfo);
        List<Model.Dashboard> GetDashboardList(BusinessUser UserInfo);
    }
}