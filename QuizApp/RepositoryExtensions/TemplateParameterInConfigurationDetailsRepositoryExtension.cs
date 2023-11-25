using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class TemplateParameterInConfigurationDetailsRepositoryExtension
    {
        public static IEnumerable<TemplateParameterInConfigurationDetails> GetTemplateParameterInConfigurationDetailsByConfigurationId(this GenericRepository<TemplateParameterInConfigurationDetails> repository, int configurationDetailsId)
        {
            return repository.Get(r => r.ConfigurationDetailsId == configurationDetailsId);
        }
    }
}