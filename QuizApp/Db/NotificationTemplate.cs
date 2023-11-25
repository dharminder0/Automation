using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class NotificationTemplate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string Title { get; set; }

        public string OfficeId { get; set; }

        public int NotificationType { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public string SMSText { get; set; }

        public string WhatsApp { get; set; }

        public int Status { get; set; }

        public int State { get; set; }

        public string EmailLinkVariable { get; set; }

        public DateTime CreatedOn { get; set; }

        public int CreatedBy { get; set; }

        public DateTime? LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }

        public string MsgVariables { get; set; }

        [ForeignKey("Company")]
        public int? CompanyId { get; set; }
        public virtual Company Company { get; set; }

        public virtual ICollection<NotificationTemplatesInQuiz> NotificationTemplatesInQuiz { get; set; }

        public virtual ICollection<AttachmentsInNotificationTemplate> AttachmentsInNotificationTemplate { get; set; }
    }
}