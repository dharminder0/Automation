using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using NLog;
using QuizApp.Helpers;
using QuizApp.Response;
using QuizApp.Services.Service;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/WhatsApp")]
    [SwaggerResponseRemoveDefaults]
    public class WhatsAppTemplateController : BaseController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private IWhatsAppService _whatsAppService;

        public WhatsAppTemplateController(IWhatsAppService whatsAppService)
        {
            _whatsAppService = whatsAppService;
        }

        /// <summary>
        /// Get WhatsApp HSM Templates by clientCode
        /// </summary>
        /// <param name="clientCode"></param>
        /// <param name="templatesType"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetHSMTemplates")]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(object))]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetWhatsAppHSMTemplates(string templatesType, string language = null)
        {
            var response = _whatsAppService.GetWhatsAppHSMTemplates(CompanyInfo.ClientCode, templatesType, true, language);
            return Json(response);
        }




        /// <summary>
        /// Get WhatsApp Templates Languages
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetLanguages")]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetWhatsAppTemplatesLanguages(string language = null, string templateType=null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(templateType))
                {
                    templateType = "Automation";
                }

                var listObj = _whatsAppService.WhatsAppTemplatesLanguages(CompanyInfo.ClientCode, templateType, true, language);

                if (_whatsAppService.Status == ResultEnum.Ok)
                {

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = listObj, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = listObj, message = _whatsAppService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message }));
            }
        }

        /// <summary>
        /// Add Quiz Question
        /// </summary>
        /// <param name="QuizId"></param>
        /// <param name="templateId"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddQuizWhatsAppTemplate")]
        [AuthorizeTokenFilter]
        public IHttpActionResult AddQuizWhatsAppTemplate(int QuizId, int templateId, string language)
        {
            QuizQuestionDetailsResponse response = null;
            try
            {
                var quizQuestionDetailsObj = _whatsAppService.AddQuizWhatsAppTemplate(QuizId, templateId, language, BusinessUserId, CompanyInfo.Id, CompanyInfo.ClientCode, (int)BranchingLogicEnum.WHATSAPPTEMPLATE);

                if (_whatsAppService.Status == ResultEnum.Ok)
                {
                    response = new QuizQuestionDetailsResponse();

                    response = (QuizQuestionDetailsResponse)response.MapEntityToResponse(quizQuestionDetailsObj);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = response, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = response, message = _whatsAppService.ErrorMessage }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = response, message = ex.Message }));
            }
        }

        /// <summary>
        /// Get Communication Contact info for Dynamic Variables
        /// </summary>
        /// <param name="clientCode"></param>
        /// <param name="contactId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCommContactDetails")]
        [SwaggerResponse(HttpStatusCode.OK, null, typeof(object))]
        [AuthorizeTokenFilter]
        public IHttpActionResult GetContactInfoWhatsapp(string contactId, string clientCode)
        {
            var response = _whatsAppService.GetCommContactDetails(contactId, clientCode);
            return Json(response);
        }


        /// <summary>
        /// Get HSM Template Details
        /// </summary>
        /// <param name="clientCode"></param>
        /// <param name="moduleType"></param>
        /// <param name="languageCode"></param>
        /// <param name="templateId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("HSMTemplateDetails")]
        [AuthorizeTokenFilter]
        public IHttpActionResult HSMTemplateDetails(string clientCode,string moduleType, string languageCode, int templateId) {
            var response = _whatsAppService.HSMTemplateDetails(clientCode, moduleType, languageCode, templateId);
            var serializer = new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            var json = JObject.FromObject(response, serializer);
            return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = json, message = string.Empty }));
        }


    }
}
