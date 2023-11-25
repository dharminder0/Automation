using Newtonsoft.Json;
using NLog;
using QuizApp.Helpers;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Service;
using Swashbuckle.Swagger.Annotations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Ignite.Security.Lib;
using Core.Common.Extensions;

namespace QuizApp.Controllers
{
    [RoutePrefix("api/v1/Account")]
    [SwaggerResponseRemoveDefaults]
    public class AccountController : ApiController
    {
        public NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        private IBusinessUserService _iBusinessUserService;

        public AccountController(IBusinessUserService iBusinessUserService)
        {
            _iBusinessUserService = iBusinessUserService;
        }

        /// <summary>
        /// To authorize Jwt Token
        /// </summary>
        /// <param name="JwtToken"></param>
        /// <param name="CompanyCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ValidateJwtToken")]
        [Route("~/api/Account/ValidateJwtToken")]
        public IHttpActionResult ValidateJwtToken(string JwtToken, string CompanyCode, string Module = "")
         {
            BusinessUserResponse businessUserResponse = null;
            try
            {
                if (!string.IsNullOrEmpty(JwtToken))
                {
                    var UserToken = string.Empty;

                    try
                    {
                        var decryptedJwtToken = JwtSecurityService.Decrypt(GlobalSettings.owcSSOSecretKeyForDecrypt.ToString(), JwtToken.Replace(" ", "+"));
                        UserToken = JwtSecurityService.Decode(decryptedJwtToken);
                    }
                    catch (Exception ex)
                    {
                        ErrorLog.LogError(ex);
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = string.Empty, message = "Invalid JwtToken" }));
                    }

                    var response = OWCHelper.ValidateUserToken(Module, UserToken, CompanyCode, true);

                    if (response != null && response.OWCBusinessUserResponse != null && string.IsNullOrEmpty(response.Error))
                    {
                        _iBusinessUserService.SaveUpdateUserInfo(OWCHelper.MapOWCResponseToEntity(response));

                        if (_iBusinessUserService.Status == ResultEnum.Ok)
                        {
                            var companyObj = new CompanyModel() { JobRocketApiUrl = response.OWCCompanyResponse.jobRocketApiUrl, JobRocketApiAuthorizationBearer = response.OWCCompanyResponse.jobRocketApiAuthorizationBearer, ClientCode = response.OWCCompanyResponse.clientCode };
                            //var domainList = OWCHelper.GetClientDomains(companyObj);
                            var domainList = CommonStaticData.GetCachedClientDomains(companyObj);
                            var domain = (domainList != null && domainList.Any()) ? domainList.FirstOrDefault().Domain : GlobalSettings.defaultDomain.ToString();
                            
                            businessUserResponse = new BusinessUserResponse();

                            businessUserResponse = businessUserResponse.MapEntityToResponse(response.OWCBusinessUserResponse, UserToken, response.OWCCompanyResponse, Module, domain);
                        }

                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = businessUserResponse, message = string.Empty }));
                    }
                    else
                    {
                        //if (response != null && !string.IsNullOrEmpty(response.Error))
                        if (!string.IsNullOrWhiteSpace(response?.Error))
                        {
                            return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = string.Empty, message = response.Error }));
                        }
                        else if (response != null && response.Status == ResultEnum.Error)
                        {
                            return ResponseMessage(Request.CreateResponse(HttpStatusCode.NotImplemented, new { data = string.Empty, message = "Not allowed" }));
                        }
                        else
                        {
                            if (response != null)
                            {
                                return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = response.OWCCompanyResponse.jobRocketClientUrl, message = response.Error }));
                            }
                        }
                        return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = GlobalSettings.webLoginURL, message = "Invalid token" }));
                    }
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = string.Empty, message = "No JwtToken found" }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = businessUserResponse, message = ex.Message }));
            }
        }

        /// <summary>
        /// To authorize Company Code
        /// </summary>
        /// <param name="CompanyCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ValidateCompanyToken")]
        public IHttpActionResult ValidateCompanyCode(string CompanyCode)
        {
            try
            {
                var UserToken = string.Empty;

                //var response = OWCHelper.GetCompanyInfo(CompanyCode);
                var response = CommonStaticData.GetCachedCompanyInfo(CompanyCode);

                if (response != null)
                {
                    var companyResponse = new CompanyResponse();

                    companyResponse = companyResponse.MapEntityToResponse(response);

                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.OK, new { data = companyResponse, message = string.Empty }));
                }
                else
                {
                    return ResponseMessage(Request.CreateResponse(HttpStatusCode.Unauthorized, new { data = string.Empty, message = "Invalid CompanyCode" }));
                }
            }
            catch (Exception ex)
            {
                ErrorLog.LogError(ex);
                return ResponseMessage(Request.CreateResponse(HttpStatusCode.InternalServerError, new { data = string.Empty, message = ex.Message }));
            }
        }

        /// <summary>
        /// Gets list of countries selected for a client
        /// </summary>
        /// <param name="clientCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClientCountries")]
        public IHttpActionResult GetClientCountries(string clientCode)
        {
            var list = _iBusinessUserService.GetClientCountries(clientCode);
            return Json(list);
        }

    }
}
