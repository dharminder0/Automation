using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Helpers
{
    public class Models
    {
        public class Offices
        {
            public string id { get; set; }
            public string name { get; set; }
            public List<child> children { get; set; }
            public class child
            {
                public string id { get; set; }
                public string name { get; set; }
            }
        }

        public class HeaderMenus
        {
            public string Culture { get; set; }
            public List<SubMenus> Menus { get; set; }
            public class SubMenus
            {
                public int Level { get; set; }
                public string MenuText { get; set; }
                public string MenueUrl { get; set; }
            }
        }

        public class HeaderMenusNew
        {
            public string Culture { get; set; }
            public List<SubMenu> Menus { get; set; }
        }

        public class SubMenu
        {
            public string name { get; set; }
            public string url { get; set; }
            public string placement { get; set; }
            public int sortOrder { get; set; }
            public bool isHighlight { get; set; }
            public List<SubMenu> subMenus { get; set; }
        }

        public class WorkPackage
        {
            public string LeadUserId { get; set; }
            public int AppointmentTypeId { get; set; }
            public int? BusinessUserId { get; set; }
            public int? CampaignId { get; set; }
            public string CampaignName { get; set; }
        }


        public class AppointmentWorkPackage
        {
            public string LeadUserId { get; set; }
            public int AppointmentTypeId { get; set; }
            public int? BusinessUserId { get; set; }
            public string SourceId { get; set; }
            public string SourceName { get; set; }
            public bool IsUpdatedSend { get; set; }
            public string Subject { get; set; }
            public string Body { get; set; }
            public string SMSText { get; set; }
            public string CompanyCode { get; set; }
            public List<int> CalendarIds { get; set; }
        }

        public class CloudinarySetting
        {
            public string signature { get; set; }
            public string CloudName { get; set; }
            public string ApiKey { get; set; }
            public string Username { get; set; }
            public long Timestamp { get; set; }
        }

        public class FileAttachment
        {
            public string FileName { get; set; }
            public string FileLink { get; set; }
        }

        public class GtmSetting
        {
            public int Id { get; set; }
            public string Brand { get; set; }
            public string GtmCode { get; set; }
        }
    }
}