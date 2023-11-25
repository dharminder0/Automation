using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace QuizApp.Db
{
    public class AttachmentsInNotificationTemplate
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("NotificationTemplate")]
        public int NotificationTemplateId { get; set; }
        public virtual NotificationTemplate NotificationTemplate { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public string  FileIdentifier { get; set; }

        public DateTime LastUpdatedOn { get; set; }

        public int? LastUpdatedBy { get; set; }
    }
}