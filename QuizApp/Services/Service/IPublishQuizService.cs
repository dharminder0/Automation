using QuizApp.Helpers;

namespace QuizApp.Services.Service
{
    public interface IPublishQuizService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        string PublishQuiz(int QuizId, int BusinessUserId, int CompanyId);
    }
}