using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service
{
    public interface IAutomationDetailsService {
        string ErrorMessage { get; set; }
        ResultEnum Status { get; set; }
        AutomationDetail GetAutomationDetail(int quizId, string ClientCode);
        bool DeleteAutomationLog(string LeadUserId, string ConfigurationId);
    }
}
