using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Response
{
    public class ReminderSettingResponse : IResponse
    {
        public string OfficeId { get; set; }
        public int? FirstReminder { get; set; }
        public int? SecondReminder { get; set; }
        public int? ThirdReminder { get; set; }

        public IResponse MapEntityToResponse(Base obj)
        {
            ReminderSettingResponse reminderSettingResponse = new ReminderSettingResponse();
            var reminderSettingObj = (ReminderSetting)obj;

            if (reminderSettingObj != null)
            {
                reminderSettingResponse.OfficeId = reminderSettingObj.OfficeId;
                reminderSettingResponse.FirstReminder = reminderSettingObj.FirstReminder;
                reminderSettingResponse.SecondReminder = reminderSettingObj.SecondReminder;
                reminderSettingResponse.ThirdReminder = reminderSettingObj.ThirdReminder;
            }

            return reminderSettingResponse;
        }
    }
}