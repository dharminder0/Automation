using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizBrandingAndStyleRepositoryExtension
    {
        public static IEnumerable<QuizBrandingAndStyle> GetQuizBrandingAndStyleByQuizId(this GenericRepository<QuizBrandingAndStyle> repository, int quizDetailId)
        {
            return repository.Get(r => r.QuizId == quizDetailId);
        }
    }
}