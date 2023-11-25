using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuizApp.Services.Model;

namespace QuizApp.Services.Service
{
    public interface ITemplateService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        QuizTemplate GetTemplateList(string QuizType, string CategoryId, int PageNo, int PageSize);
        QuizTemplate GetDashboardTemplates(long OffsetValue, string QuizTypeId, string CategoryId, string SearchTxt, int PageNo, int PageSize, int OrderByDate, int companyId, string ClientCode);
        void SetTemplateStatus(int Id, int TemplateStatus, int BusinessUserId);
    }
}
