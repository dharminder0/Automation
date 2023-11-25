using QuizApp.Db;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class QuizVariablesRepositoryExtension
    {

        public static IEnumerable<QuizVariables> GetQuizVariablesByObjectId(this GenericRepository<QuizVariables> repository, int objectId, int objectTypes, int quizDetailsId)
        {
            return repository.Get(v => v.ObjectId == objectId && v.ObjectTypes == objectTypes && v.QuizDetailsId == quizDetailsId);
        }
        public static IEnumerable<QuizVariables> GetQuizVariablesByQuizId(this GenericRepository<QuizVariables> repository, int quizDetailId)
        {
            return repository.Get(r => r.QuizDetailsId == quizDetailId);
        }

    }
}