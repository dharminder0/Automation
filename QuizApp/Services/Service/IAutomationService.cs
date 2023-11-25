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
    public interface IAutomationService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        QuizResult AddQuizResult(int QuizId, int BusinessUserId, int CompanyId, int? quizType = 0);
        QuizDetailsModel GetQuizDetails(int QuizId);
        QuizAnswerSubmit AttemptQuiz(List<TextAnswer> TextAnswerList, string QuizCode, string Mode, string Type, int QuestionId = -1, string AnswerId = "", int BusinessUserId = 1, int UserTypeId = 0, int? QuestionType = (int)BranchingLogicEnum.QUESTION, int? UsageType = null);
        AttemptedQuizDetails GetAttemptedQuizDetails(string  LeadUserId, int QuizId, string ConfigurationId);
        AttemptedQuizDetails GetAttemptedQuizDetailsV2(string LeadUserId, int QuizId, string ConfigurationId);
    }
}
