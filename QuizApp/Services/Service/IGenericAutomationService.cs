using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service
{
    public interface IGenericAutomationService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }

        int CreateQuiz(LocalQuiz QuizObj, int BusinessUserId, bool IsCreateAcademyCourse, bool CreateTechnicalRecruiterCourse, int companyId);

    }
}
