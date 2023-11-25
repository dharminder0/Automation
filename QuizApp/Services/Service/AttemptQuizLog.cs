using Core.Common.Extensions;
using Newtonsoft.Json;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;
using static QuizApp.Helpers.Models;

namespace QuizApp.Services.Service
{
    public static class AttemptQuizLog
    {
        public static ResultEnum Status { get; private set; }
        public static string ErrorMessage { get; private set; }

        public static int InsertLogging(int quizId, string leadId, int quizAttemptId, DateTime createdOn, string requestJson)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var logging = new Db.AttemptQuizLog()
                    {
                        QuizId = quizId,
                        LeadId = leadId,
                        QuizAttemptId = quizAttemptId,
                        CreatedOn = createdOn,
                        RequestJson = requestJson.SerializeObjectWithoutNull(),
                    };

                    UOWObj.AttemptQuizLogRepository.Insert(logging);
                    UOWObj.Save();
                    return logging.Id;
                }
            }
            catch(Exception ex)
            {
                return 0;

            }

            return 0;
        }

        public static void UpdateResponseJson(int id, string responseJson)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (id != 0 && !string.IsNullOrWhiteSpace(responseJson))
                    {
                        var logging = UOWObj.AttemptQuizLogRepository.GetByID(id);
                        logging.ResponseJson = responseJson.SerializeObjectWithoutNull();
                        UOWObj.AttemptQuizLogRepository.Update(logging);
                        UOWObj.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                return ;

            }
            return;
        }
    }

}