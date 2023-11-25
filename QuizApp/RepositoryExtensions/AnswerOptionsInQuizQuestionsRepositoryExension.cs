using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public  static class AnswerOptionsInQuizQuestionsRepositoryExension
    {
        public static IEnumerable<AnswerOptionsInQuizQuestions> GetAnswerOptionsInQuizQuestionsByQuestionId(this GenericRepository<AnswerOptionsInQuizQuestions> repository, int id, int status)
        {
            return repository.Get(r => r.QuestionId == id && r.Status == status);
        }
        public static IEnumerable<AnswerOptionsInQuizQuestions> GetAnswerOptionsInQuizQuestionsByStatus(this GenericRepository<AnswerOptionsInQuizQuestions> repository,int status, int id)
        {
            return repository.Get(a => a.Status == status && a.Id == id);
        }
    }
}

