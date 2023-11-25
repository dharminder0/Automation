using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class AttachmentsInConfigurationRepositoryExtension
    {
        public static IEnumerable<AttachmentsInConfiguration> GetAttachmentsInConfigurationByConfigurationId(this GenericRepository<AttachmentsInConfiguration> repository, int configurationDetailId)
        {
            return repository.Get(v => v.ConfigurationDetailsId == configurationDetailId);
        }
     
    }
}
