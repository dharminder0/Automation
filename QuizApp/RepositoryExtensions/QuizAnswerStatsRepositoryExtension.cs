using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public  static class QuizAnswerStatsRepositoryExtension
    {
        public static IEnumerable<QuizAnswerStats> GetQuizAnswerStatsByAnswerId(this GenericRepository<QuizAnswerStats> repository, int id, int quizQuestionStatsId)
        {
            return repository.Get(r => r.AnswerId == id && r.QuizQuestionStatsId == quizQuestionStatsId);
        }
    }
}
