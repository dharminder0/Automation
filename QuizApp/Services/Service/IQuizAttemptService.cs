using QuizApp.Helpers;
using QuizApp.Services.Model;

namespace QuizApp.Services.Service
{
    public interface IQuizAttemptService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        AttemptQuizSetting AttemptQuizSettings(string quizCode);
        QuizAnswerSubmit AttemptQuiz(AttemptQuizRequest attemptQuizRequest);
        QuizAnswerSubmit FailedQuiz(FailedQuiz failedQuiz);
        QuizAnswerSubmit NotCompletedQuiz(PublishQuizTmpModel publishQuizTmpModel);
    }
}