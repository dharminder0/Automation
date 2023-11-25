using QuizApp.Helpers;

namespace QuizApp.Services.Service {
    public interface IUncompleteQuizService {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        void UpdateUncompleteService(object state);
        void MarkQuizIncomplete(int quizAttemptId);
    }
}