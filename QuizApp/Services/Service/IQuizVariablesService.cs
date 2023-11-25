using QuizApp.Helpers;
using QuizApp.Services.Model;
using System.Collections.Generic;

namespace QuizApp.Services.Service
{
    public interface IQuizVariablesService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }

        QuizVariableModel AddQuizVariables(QuizVariableModel quizVariable);
        List<string> GetQuizVariables(int quizDetailsId, int objectid, int objectType);
        //void UpdateQuizVariables(QuizVariableModel quizVariable);
    }
}