using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service
{
    public interface IQuestionService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        void UpdateAnswerType(int QuestionId, int AnswerType, int BusinessUserId, int CompanyId, int? answerStructureType, bool isWhatsappEnable = false, bool isMultiRating = false);
        QuizQuestion AddQuizQuestion(int QuizId, int BusinessUserId, int CompanyId, int Type, bool isWhatsappEnable = false);
        QuestionMappedValue GetQuestionMappedFieldValues(int QuizId, string LeadUserId);
    }
}