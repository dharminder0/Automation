using QuizApp.Helpers;
using QuizApp.Services.Model;

namespace QuizApp.Services.Service
{
    public interface IQuizCoverService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }

        QuizCover GetQuizCoverDetails(int QuizId);
        void UpdateQuizCoverDetails(QuizCover QuizCoverObj, int BusinessUserId, int CompanyId);
        void UpdateQuizCoverImage(QuizCover QuizCoverObj, int BusinessUserId, int CompanyId);
        void UpdateCoverSettingtInQuiz(QuizCover QuizCorrectAnswerSettingObj, int BusinessUserId, int CompanyId);

    }
}
