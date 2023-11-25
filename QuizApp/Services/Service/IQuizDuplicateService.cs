using QuizApp.Helpers;
using QuizApp.Request;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuizApp.Services.Service
{
    public interface IQuizDuplicateService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        QuizQuestion AddQuizDuplicateQuestion(int quizId, int questionId, int businessUserId, int companyId);
        QuizResult AddQuizDuplicateResult(int quizId, int quizResultId, int businessUserId, int companyId, int? quizType = 0);
        int DuplicateQuizByCompanyId(DuplicateQuizRequest duplicateQuizRequest);
    }
}