using QuizApp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service
{
    public interface IApiUsageLogsService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }

        void Save(string controller, string action, string url, string body, DateTime requestDate, string response);
        void RunDeleteLogsService(object state);
        int ClearApiUsageLogs();
        void DeletePerivewQuizAttempts(int attemptId);
    }
}
