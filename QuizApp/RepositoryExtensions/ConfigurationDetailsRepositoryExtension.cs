using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class ConfigurationDetailsRepositoryExtension
    {
        public static IEnumerable<ConfigurationDetails> GetConfigurationDetailsByConfigurationId(this GenericRepository<ConfigurationDetails> repository, string configurationId)
        {
            return repository.Get(v => v.ConfigurationId == configurationId);
        }
        public static IEnumerable<ConfigurationDetails> GetConfigurationDetailsByQuizId(this GenericRepository<ConfigurationDetails> repository, int quizId)
        {
            return repository.Get(v => v.QuizId == quizId);
        }
        public static IEnumerable<ConfigurationDetails> GetConfigurationDetailsById(this GenericRepository<ConfigurationDetails> repository, int configurationDetailId)
        {
            return repository.Get(v => v.Id == configurationDetailId);
        }
    }
}
