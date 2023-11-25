using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service
{
    public interface IReminderSettingService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        ReminderSetting GetReminderSettings(string OfficeId, int CompanyId);
        void SaveReminderSettings(ReminderSetting reminderSetting, int BusinessUserId , int CompanyId);
        List<ReminderQueuesModel> GetReminderQueue();
        void UpdateReminderQueueStatus(ReminderQueuesModel reminderQueue);
    }
}
