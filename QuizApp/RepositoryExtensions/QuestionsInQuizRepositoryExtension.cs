using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuestionsInQuizRepositoryExtension
    {

        public static IEnumerable<QuestionsInQuiz> GetExistingQuestioninQuiz(this GenericRepository<QuestionsInQuiz> repository, int quizDetailId)
        {
            return repository.Get(v => v.QuizId == quizDetailId);
        }

        public static IEnumerable<QuestionsInQuiz> GetQuestioninQuizById(this GenericRepository<QuestionsInQuiz> repository, int status, int questionId)
        {
            return repository.Get(a => a.Status == status && a.Id == questionId);
        }
    }
}