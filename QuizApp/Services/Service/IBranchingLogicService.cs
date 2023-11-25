using QuizApp.Helpers;
using QuizApp.Services.Model;

namespace QuizApp.Services.Service
{
    public interface IBranchingLogicService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        QuizBranchingLogic GetQuizBranchingLogic(int QuizId);
        QuizBranchingLogic GetQuizBranchingLogicOld(int QuizId);
        QuizBranchingLogicLinksList GetQuizBranchingLogicData(int QuizId);

    }
}
