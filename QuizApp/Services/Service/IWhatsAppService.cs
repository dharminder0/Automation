using QuizApp.Helpers;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace QuizApp.Services.Service
{
    public interface IWhatsAppService
    {
        ResultEnum Status { get; set; }
        string ErrorMessage { get; set; }
        object GetWhatsAppHSMTemplates(string clientCode, string templatesType, bool replaceParameters = true, string language = null);
        object WhatsAppTemplatesLanguages(string clientCode, string type = null, bool replaceParameters = true, string language = null);
        QuizQuestion AddQuizWhatsAppTemplate(int QuizId, int templateId, string language, int BusinessUserId, int CompanyId, string clientCode, int Type);

        IDictionary<string, object> GetCommContactDetails(string contactId, string clientCode);
        Response.WhatsappTemplateV3.GetWhatsappTemplateV3 HSMTemplateDetails(string clientCode, string moduleType, string languageCode, int templateId);

        // List<Db.Languages> WhatsAppTemplatesLanguages();
    }
}