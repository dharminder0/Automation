using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public  static class PersonalityResultSettingRepositoryExtension
    {
        public static IEnumerable<PersonalityResultSetting> GetPersonalityResultSettingByQuizId(this GenericRepository<PersonalityResultSetting> repository, int quizDetailId)
        {
            return repository.Get(r => r.QuizId == quizDetailId);
        }
    }
}





