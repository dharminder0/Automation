using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class BranchingLogicRepositoryeExtension
    {
        public static IEnumerable<BranchingLogic> GetBranchingLogicByQuizId(this GenericRepository<BranchingLogic> repository, int quizdetailId)
        {
            return repository.Get(r => r.QuizId == quizdetailId);
        }
    }
}