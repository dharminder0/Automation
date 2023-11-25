using QuizApp.Helpers;
using QuizApp.Request;
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
    [RoutePrefix("api/v1/Contact")]
    [SwaggerResponseRemoveDefaults]
    public class ContactController : BaseController {
        private readonly ContactService _contactService;

        public ContactController(ContactService contactService) {
            _contactService = contactService;


        }

        /// <summary>
        ///Delete Contact Data
        /// </summary>
        /// <param name="contactRequest"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteContactData")]
        [AuthorizeTokenFilter]
        public IHttpActionResult DeleteContactData(ContactRequest contactRequest ) {
            try {
                _contactService.DeleteContactData(contactRequest);

                if (_contactService.Status == ResultEnum.Ok) {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = true, message = string.Empty }));
                } else {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = false, message = _contactService.ErrorMessage }));
                }
            } catch (Exception ex) {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }
    }
}
