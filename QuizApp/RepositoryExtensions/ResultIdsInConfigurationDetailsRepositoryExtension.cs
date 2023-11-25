using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class ResultIdsInConfigurationDetailsRepositoryExtension
    {
        public static IEnumerable<ResultIdsInConfigurationDetails> GetResultIdsInConfigurationDetailsByConfigurationDetailsId(this GenericRepository<ResultIdsInConfigurationDetails> repository, int configurationDetailsId)
        {
            return repository.Get(v => v.ConfigurationDetailsId == configurationDetailsId);
        }
    }
}