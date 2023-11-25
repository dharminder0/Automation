using NLog;
using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Web;

namespace QuizApp.Services.Service
{
    public class ConfigurationDetailsLogService : IConfigurationDetailsLogService
    {

        public void ClearOldConfigurationByQuizAttemptId(int attemptId)
        {
            var deletedRecord = 0;
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var dateTime = DateTime.UtcNow.AddDays(-720);
                  // var quizStats = UOWObj.QuizStatsRepository.Get(r => r.QuizAttemptId == attemptId && r.ResultId != null && r.CompletedOn != null && r.CompletedOn < dateTime);
                    var quizStats = UOWObj.QuizStatsRepository.Get(r => r.QuizAttemptId == attemptId);
                    if (quizStats != null && quizStats.Any())
                    {
                        foreach (var item in quizStats)
                        {
                            DeleteOldQuizConfiguration(item.QuizAttemptId);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
            }
        }





        private void DeleteOldQuizConfiguration(int quizAttemptId)
        {

//delete from TemplateParameterInConfigurationDetails where configurationdetailsid = 7535
//delete from LeadDataInAction where configurationdetailsid = 7535
//delete from  VariablesDetails where configurationdetailsid = 7535
//delete from MediaVariablesDetails where configurationdetailsid = 7535
//delete from ResultIdsInConfigurationDetails where configurationdetailsid = 7535
//delete from AttachmentsInConfiguration where configurationdetailsid = 7535
//update WorkPackageInfo set configurationdetailsid = null  where configurationdetailsid = 7535
//update quizattempts set configurationdetailsid = null  where configurationdetailsid = 7535
//delete from configurationdetails where id = 7535




            using (var UOWObj = new AutomationUnitOfWork())
            {
                if (quizAttemptId != null)
                {
                    var quizObj = UOWObj.QuizAttemptsRepository.Get(r => r.Id == quizAttemptId);
                    if (quizObj != null && quizObj.Any())
                    {
                        foreach(var item in quizObj.Where(v => v.ConfigurationDetailsId != null))
                        {
                            int configurationId = item.ConfigurationDetailsId.Value;
                            if (configurationId != 0)
                            {
                                string query = $@"

                                delete from TemplateParameterInConfigurationDetails where configurationdetailsid ={configurationId};
                                delete  from LeadDataInAction where configurationdetailsid ={configurationId};
                                delete  from  VariablesDetails where configurationdetailsid ={configurationId} ;
                                delete  from MediaVariablesDetails where configurationdetailsid ={configurationId};
                                delete  from ResultIdsInConfigurationDetails where configurationdetailsid ={configurationId} ;
                                delete  from AttachmentsInConfiguration where configurationdetailsid ={configurationId} ;
                                update WorkPackageInfo set configurationdetailsid =null  where configurationdetailsid ={configurationId} ;
                                update quizattempts set configurationdetailsid =null  where configurationdetailsid ={configurationId} ;
                                delete from configurationdetails where id ={configurationId} 
                                ";
                                UOWObj.QuizAttemptsRepository.DeleteRange(query);
                                UOWObj.Save();
                            }

 
                        }
                    }
                }
            }
        }
    }
}