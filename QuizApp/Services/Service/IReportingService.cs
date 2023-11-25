using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service
{
    public interface IReportingService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }

        List<QuizReport> GetQuizReportDetails(string QuizIdCSV, DateTime fromDate, DateTime toDate, string numerator, string denominator);
        Report GetQuizReport(int QuizId, string SourceId, DateTime? FromDate, DateTime? ToDate, int? ResultId, int CompanyId);
        LeadReportDetails GetQuizLeadReport(int QuizId, string LeadUserId, int CompanyId);
        NPSReport GetNPSAutomationReport(int QuizId, string SourceId, int ChartView, DateTime? FromDate, DateTime? ToDate, int? ResultId, int CompanyId);

        Report GetQuizTemplateReport(string TemplateId, DateTime? FromDate, DateTime? ToDate, int? ResultId, CompanyModel CompanyObj);
        LeadReportDetails GetQuizTemplateLeadReport(string TemplateId, string LeadUserId, CompanyModel CompanyObj);
        NPSReport GetNPSTemplateAutomationReport(string TemplateId, int ChartView, DateTime? FromDate, DateTime? ToDate, int? ResultId, CompanyModel CompanyObj);
        TemplatateDetail GetTemplateQuizDetails(string TemplateId, CompanyModel CompanyObj);
    }
}
