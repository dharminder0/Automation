using Core.Common.Extensions;
using Newtonsoft.Json;
using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace QuizApp.Services.Service
{
    public static class ExternalActionQueueService
    {
        private readonly static int InProgress = (int)QueueStatusTypes.InProgress;
        private readonly static int Error = (int)QueueStatusTypes.Error;
        private readonly static int Done = (int)QueueStatusTypes.Done;
        public static void TriggerAction()
        {

            var queueItem = GetBufferedQueueItems();
            if (queueItem != null && queueItem.Any())
            {
                foreach(var item in queueItem)
                {
                    switch (item.ItemType)
                    {
                        case QueueItemTypes.InsertQuizAttemptLead:
                            TriggerSyncQuizAttemptWithlead(item);
                            break;

                        case QueueItemTypes.InsertSaveLeadTags:
                            TriggerSaveLeadTagsTask(item);
                            break;

                        case QueueItemTypes.SendEmailSms:
                            TriggerSendEmailSmsNew(item);
                            break;

                        case QueueItemTypes.SendWhatsapp:
                            TriggerSendWhatsappNew(item);
                            break;

                    }
                }
            }


        }

        public static void TriggerSyncQuizAttemptWithlead(ExternalActionQueue externalActionQueue)
        {
            var objectJson = !string.IsNullOrWhiteSpace(externalActionQueue.ObjectJson) ? JsonConvert.DeserializeObject<PublishQuizTmpModel>(externalActionQueue.ObjectJson) : null;
            if (objectJson != null)
            {
                try
                {
                    UpdateQueueItem(externalActionQueue, InProgress);
                    var payload = objectJson;
                    QuizAttemptServiceBase.SyncQuizAttemptWithlead(payload);
                    UpdateQueueItem(externalActionQueue, Done);
                }
                catch (Exception ex)
                {
                    UpdateQueueItem(externalActionQueue, Error);
                }
                
            }
        }

        public static void TriggerSaveLeadTagsTask(ExternalActionQueue externalActionQueue)
        {
            var objectJson = !string.IsNullOrWhiteSpace(externalActionQueue.ObjectJson) ? JsonConvert.DeserializeObject<PublishQuizTmpModel>(externalActionQueue.ObjectJson) : null;
            if (objectJson != null)
            {
                try
                {
                    UpdateQueueItem(externalActionQueue, InProgress);
                    var payload = objectJson;
                    QuizAttemptServiceBase.SaveLeadTagsTask(payload);
                    UpdateQueueItem(externalActionQueue, Done);
                }
                catch (Exception ex)
                {
                    UpdateQueueItem(externalActionQueue, Error);
                }

            }
        }
        public static void TriggerSendEmailSmsNew(ExternalActionQueue externalActionQueue)
        {
            var objectJson = !string.IsNullOrWhiteSpace(externalActionQueue.ObjectJson) ? JsonConvert.DeserializeObject<TempWorkpackagePush>(externalActionQueue.ObjectJson) : null;
            if (objectJson != null)
            {
                try
                {
                    UpdateQueueItem(externalActionQueue, InProgress);
                    var payload = objectJson;
                    Task.Run(async () => {
                        await WorkpackageCommunicationService.SendEmailSmsNew(payload);
                    });
                    UpdateQueueItem(externalActionQueue, Done);
                }
                catch (Exception ex)
                {
                    UpdateQueueItem(externalActionQueue, Error);
                }

            }

        }

        public static void TriggerSendWhatsappNew(ExternalActionQueue externalActionQueue)
        {
            var objectJson = !string.IsNullOrWhiteSpace(externalActionQueue.ObjectJson) ? JsonConvert.DeserializeObject<TempWorkpackagePush>(externalActionQueue.ObjectJson) : null;
            if (objectJson != null)
            {
                try
                {
                    UpdateQueueItem(externalActionQueue, InProgress);
                    var payload = objectJson;
                    Task.Run(async () => {
                        await WorkpackageCommunicationService.SendWhatsappNew(payload);
                    });
                    UpdateQueueItem(externalActionQueue, Done);
                }
                catch (Exception ex)
                {
                    UpdateQueueItem(externalActionQueue, Error);
                }

            }

        }



        private static void UpdateQueueItem(ExternalActionQueue item, int status)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                item.Status = status;
                item.ModifiedOn = DateTime.UtcNow;
                UOWObj.ExternalActionQueueRepository.Update(item);
                UOWObj.Save();
            }
        }
        public static long InsertExternalActionQueue(int companyId, string objectId , string ItemType, int status, string request)
            {
                try
                {
                    using (var UOWObj = new AutomationUnitOfWork())
                    {
                        var externalActionQueueObj = new Db.ExternalActionQueue
                        {
                            AddedOn = DateTime.UtcNow,
                            CompanyId = companyId,
                            ObjectId = objectId,
                            ItemType = ItemType,
                            ObjectJson = request.SerializeObjectWithoutNull(),
                            Status = status,
                            ModifiedOn = DateTime.UtcNow
                        };
                        UOWObj.ExternalActionQueueRepository.Insert(externalActionQueueObj);
                        UOWObj.Save();
                        return externalActionQueueObj.Id;
                    }
                }
                catch(Exception ex){
                    return 0;
                }
                return 0;
            }

        public static void UpdateQueueItem(int id, string responseJson)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    if (id != 0 && !string.IsNullOrWhiteSpace(responseJson))
                    {
                        var externalActionQueue = UOWObj.ExternalActionQueueRepository.GetByID(id);
                        externalActionQueue.ObjectJson = responseJson.SerializeObjectWithoutNull();
                        UOWObj.ExternalActionQueueRepository.Update(externalActionQueue);
                        UOWObj.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                return;

            }
            return;
        }

        public static void DeleteActionQueue(long queueId)
        {
            using (var UOWObj = new AutomationUnitOfWork())
            {
                var actionQueue = UOWObj.ExternalActionQueueRepository.Get(r => r.Id == queueId);

                if (actionQueue.Any())
                {
                    foreach (var obj in actionQueue)
                    {
                        UOWObj.ExternalActionQueueRepository.Delete(obj);
                    }
                }
                UOWObj.Save();
            }
        }

        public static IEnumerable<ExternalActionQueue> GetBufferedQueueItems()
        {
            IEnumerable<ExternalActionQueue> externalActionQueues = new List<ExternalActionQueue>();
            try
            {
               // IEnumerable<ExternalActionQueue> externalActionQueues = null;
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var actionQueue = UOWObj.ExternalActionQueueRepository.GetSelectedColoumnV2(r => new { r.Id }, null, null, null, 10);
                    return (IEnumerable<ExternalActionQueue>)actionQueue;
                }
            }
            catch (Exception ex)
            {
                return externalActionQueues;
            }

        }

        //private static void GetExternalActionQueue(long queueId , AutomationUnitOfWork UOWObj)
        //{
        //    var query = $@" SELECT  TOP 2 * FROM ExternalActionQueue WHERE Id = {queueId}";
        //    UOWObj.ExternalActionQueueRepository.DeleteRange(query);

        //}
    }
}