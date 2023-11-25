using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
        public static class QuizDetailsRepositoryExtension
        {

            public static IEnumerable<QuizDetails> GetQuizDetailsbyParentQuizIdRepositoryExtension(this GenericRepository<QuizDetails> repository, int quizId)
            {
                return repository.Get(v => v.ParentQuizId == quizId);
            }

            public static IEnumerable<QuizDetails> GetQuizDetailsbyId(this GenericRepository<QuizDetails> repository, int quizdetailId)
            {
                return repository.Get(v => v.Id == quizdetailId);
            }
        }
}