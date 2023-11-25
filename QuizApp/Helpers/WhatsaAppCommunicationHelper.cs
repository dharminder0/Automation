using Core.Common.Extensions;
using Newtonsoft.Json;
using NLog;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using QuizApp.Services.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using static QuizApp.Response.QuizTemplateResponse;
using static QuizApp.Response.VariablesObjectFieldDto;

namespace QuizApp.Helpers
{
    public class WhatsaAppCommunicationHelper
    {
        public static readonly OWCHelper _owchelper = new OWCHelper();
        public static bool SendWhatsapp(string contactphone, Dictionary<string, object> exObj, Dictionary<string, object> staticobjects, string FollowUpMessage, string MsgVariables, int HsmTemplateId, string LanguageCode, int WorkPackageInfoId, string usertoken = "", string clientcode = "", List<Db.TemplateParameterInConfigurationDetails> templateParameterInConfigurationDetails = null, TempWorkpackagePush tempWorkpackagePush = null) {
            var leadRecruiterObj = new OWCBusinessUserResponse();
            string followUpMessage = "";
            var media = new List<UserMediaClassification>();
            CommonStaticData.VacancyVariableLink(exObj, FollowUpMessage, clientcode);
            if (!string.IsNullOrWhiteSpace(usertoken)) {
                CommonStaticData.UserVariableLink(exObj, FollowUpMessage, usertoken, clientcode);
            }

            followUpMessage = DynamicExtensions.DynamicVariablesReplace(FollowUpMessage, exObj, staticobjects, MsgVariables);

            var whatsappMessageObj = new WhatsappMessage();
            List<WhatsappMessage.ButtonParam> listbuttonParams = null;
            List<WhatsappMessage.TemplateParameter> listParams = null;
            List<WhatsappMessage.HeaderParameter> listheaderParams = null;
            bool isVacancyVariableExist = false;
            bool isUserVariableExist = false;
            var template = OWCHelper.WhatsAppTemplates(HsmTemplateId);
            if (template == null) {
                return false;
            }
            var whatsapptemplate = JsonConvert.DeserializeObject<WhatsappTemplate>(template.ToString());
            if (whatsapptemplate != null && whatsapptemplate.HeaderParams != null && whatsapptemplate.HeaderParams.Count != 0) {

                foreach (var itemHeaderParams in whatsapptemplate.HeaderParams) {
                    if (itemHeaderParams.moduleCode == "Automation") {
                        listheaderParams = new List<WhatsappMessage.HeaderParameter>();
                        if (itemHeaderParams.Params != null) {
                            foreach (var Headerparams in itemHeaderParams.Params) {

                                if (!isUserVariableExist && itemHeaderParams.Params.Any(v => v.Paraname.ContainsCI("user."))) {
                                    isUserVariableExist = true;
                                }
                                if (!isVacancyVariableExist && itemHeaderParams.Params.Any(v => v.Paraname.ContainsCI("vacancy."))) {
                                    isVacancyVariableExist = true;
                                }
                                listheaderParams.Add(
                                    new WhatsappMessage.HeaderParameter {
                                        paraname = Headerparams.Paraname,
                                        position = Headerparams.Position,
                                        value = Headerparams.Value
                                    });
                            }
                        }
                    }
                }
            } else if (whatsapptemplate != null && !(whatsapptemplate.HeaderParams!= null && whatsapptemplate.HeaderParams.Count != 0)) {
                var templateBody = whatsapptemplate.TemplateBody.FirstOrDefault(v => v.LangCode.EqualsCI(LanguageCode));
                UpdateDynamicMedia(clientcode, tempWorkpackagePush, ref leadRecruiterObj, ref media, ref listheaderParams, templateBody);
            }

            if (whatsapptemplate != null && whatsapptemplate.ButtonParams != null) {
                foreach (var itemButtonParams in whatsapptemplate.ButtonParams) {

                    if (itemButtonParams.ModuleCode == "Automation") {
                        listbuttonParams = new List<WhatsappMessage.ButtonParam>();
                        if (itemButtonParams.Buttons != null) {
                            foreach (var itemButton in itemButtonParams.Buttons) {
                                if (!isUserVariableExist && itemButton.Params.Any(v => v.Paraname.ContainsCI("user."))) {
                                    isUserVariableExist = true;
                                }

                                if (!isVacancyVariableExist && itemButton.Params.Any(v => v.Paraname.ContainsCI("vacancy."))) {
                                    isVacancyVariableExist = true;
                                }

                                listbuttonParams.Add(
                                new WhatsappMessage.ButtonParam {
                                    ButtonIndex = itemButton.ButtonIndex,
                                    Params = itemButton.Params.Select(v => new WhatsappMessage.TemplateParameter { paraname = v.Paraname, position = v.Position, value = v.Value }).ToList()

                                });
                            }
                        }
                    }
                }
            }
            if (whatsapptemplate != null && whatsapptemplate.Params != null) {

                var oldTempParamList = new List<TemplateParameter>();
                try {

                    //####################################################
                    if (templateParameterInConfigurationDetails != null && templateParameterInConfigurationDetails.Any()) {
                        foreach (var item in templateParameterInConfigurationDetails) {
                            if (!string.IsNullOrWhiteSpace(item.Value)) {
                                oldTempParamList.Add(new TemplateParameter {
                                    Paraname = item.ParaName,
                                    Position = item.Position,
                                    Value = item.Value
                                });
                            }
                        }
                        //#################################################

                    }
                } catch (Exception) {
                }


                foreach (var providerParams in whatsapptemplate.Params) {
                    if (providerParams.ModuleCode == "Automation") {
                        listParams = new List<WhatsappMessage.TemplateParameter>();
                        if (providerParams.Params != null) {
                            foreach (var itemParams in providerParams.Params) {
                                if (!isUserVariableExist && itemParams.Paraname.ContainsCI("user.")) {
                                    isUserVariableExist = true;
                                }
                                if (!isVacancyVariableExist && itemParams.Paraname.ContainsCI("vacancy.")) {
                                    isVacancyVariableExist = true;
                                }

                                try {

                                    if (oldTempParamList != null && oldTempParamList.Any(v => v.Paraname.ToLower().Equals(itemParams.Paraname))) {

                                        itemParams.Value = oldTempParamList.Where(v => v.Paraname.ToLower().Equals(itemParams.Paraname)).Select(v => v.Value).FirstOrDefault() ?? itemParams.Value;
                                    }

                                } catch (Exception ex) {


                                }


                                listParams.Add(new WhatsappMessage.TemplateParameter {
                                    paraname = itemParams.Paraname,
                                    position = itemParams.Position,
                                    value = itemParams.Value
                                });

                            }
                        }
                    }
                }

            }

            if (isVacancyVariableExist) {
                CommonStaticData.VacancyVariableLink(exObj, "vacancy.", clientcode);
            }
            if (isUserVariableExist) {
                if (!string.IsNullOrWhiteSpace(usertoken)) {
                    CommonStaticData.UserVariableLink(exObj, "user.", usertoken, clientcode);
                }
            }

            if (listbuttonParams != null && listbuttonParams.Any()) {
                foreach (var topitem in listbuttonParams) {
                    if (topitem.Params != null) {
                        SetButtonParamValues(exObj, staticobjects, topitem.Params, true);
                    }
                }
            }

            if (listParams != null && listParams.Any()) {
                SetButtonParamValues(exObj, staticobjects, listParams, false);
            }

            if (listheaderParams != null && listheaderParams.Any()) {
                SetHeaderParamValues(exObj, staticobjects, listheaderParams);
            }


            whatsappMessageObj.ButtonParams = listbuttonParams;
            whatsappMessageObj.TemplateParameters = listParams;
            whatsappMessageObj.HeaderParams = listheaderParams;
            whatsappMessageObj.ClientCode = clientcode;
            whatsappMessageObj.ContactPhone = contactphone;
            whatsappMessageObj.HsmTemplateId = HsmTemplateId;
            whatsappMessageObj.LanguageCode = !string.IsNullOrWhiteSpace(LanguageCode) ? LanguageCode : string.Empty;
            whatsappMessageObj.FollowUpMessage = followUpMessage;
            whatsappMessageObj.ModuleWorkPackageId = WorkPackageInfoId;
            whatsappMessageObj.ModuleName = "Automation";//ModuleTypeEnum
            whatsappMessageObj.EventType = TaskTypeEnum.Invitation.ToString();
            whatsappMessageObj.ObjectId = WorkPackageInfoId.ToString();
            whatsappMessageObj.UniqueCode = Guid.NewGuid().ToString();
            whatsappMessageObj.ContactId = !string.IsNullOrWhiteSpace(tempWorkpackagePush.LeadUserId) ? tempWorkpackagePush.LeadUserId : null;

            return CommunicationHelper.SendWhatsappMessage(whatsappMessageObj);
        }

        private static void UpdateDynamicMedia(string clientcode, TempWorkpackagePush tempWorkpackagePush, ref OWCBusinessUserResponse leadRecruiterObj, ref List<UserMediaClassification> media, ref List<WhatsappMessage.HeaderParameter> listheaderParams, TemplateBody templateBody) {
            if (templateBody != null && templateBody.IsEnabledDynamicMedia) {
                string ownerExternalId = string.Empty;
                if (templateBody.MediaVariable.MediaOwner == "CASE_OWNER") {
                    ownerExternalId = tempWorkpackagePush.leadUserInfo.SourceOwnerId;
                } else if (templateBody.MediaVariable.MediaOwner == "LEAD_OWNER") {
                    ownerExternalId = tempWorkpackagePush.leadUserInfo.LeadOwnerId;
                } else if (templateBody.MediaVariable.MediaOwner == "CONTACT_OWNER") {
                    ownerExternalId = tempWorkpackagePush.leadUserInfo.ContactOwnerId;
                }
                if (!string.IsNullOrWhiteSpace(ownerExternalId)) {
                    var externalDetails = _owchelper.GetExternalDetails(tempWorkpackagePush.companyObj.ClientCode, ownerExternalId);
                    if (externalDetails != null) {
                        var oWCBusinessUserResponse = JsonConvert.DeserializeObject<OWCBusinessUserResponse>(externalDetails);
                        if (oWCBusinessUserResponse != null) {
                            leadRecruiterObj = oWCBusinessUserResponse;
                        }
                        if (!string.IsNullOrWhiteSpace(leadRecruiterObj.token)) {
                            var userTokens = new List<string>();
                            userTokens.Add(leadRecruiterObj.token);
                            media = OWCHelper.GetUserMediaClassification(clientcode, userTokens);
                        }
                    }
                }
                
                var tempHeaderExample = templateBody?.HeaderExample.ToString();
                var tempProfileMedia = templateBody?.MediaVariable?.ProfileMedia;
                var tempHeaderMediaPublicUrl = templateBody?.HeaderMediaPublicId;

                listheaderParams = new List<WhatsappMessage.HeaderParameter>();
                if (media != null && media.Any()) {
                    foreach (var item in media.Where(v => v.Name == tempProfileMedia)) {
                        if (!string.IsNullOrWhiteSpace(item.MediaUrl)) {
                            if (!item.MediaUrl.EqualsCI(tempHeaderExample)) {
                                templateBody.HeaderExample = item.MediaUrl;
                            
                                listheaderParams.Add(new WhatsappMessage.HeaderParameter {
                                    paraname = "",
                                    position = 1,
                                    value = item.MediaUrl

                                });

                            }
                            if (!string.IsNullOrWhiteSpace(item.MediaPublicId)) {
                                if (!item.MediaPublicId.EqualsCI(tempHeaderMediaPublicUrl)) {
                                    templateBody.HeaderMediaPublicId = item.MediaPublicId;
                                }
                            }

                        }

                    }

                }
            }
        }

        private static void SetButtonParamValues(Dictionary<string, object> exObj, Dictionary<string, object> staticobjects, List<WhatsappMessage.TemplateParameter> topitem, bool isButtonParam)
        {
            foreach (var item in topitem)
            {
                string replacedItem = item.paraname;
                if (!Regex.IsMatch(replacedItem, @"%([a-zA-Z0-9.])+%(?![^<]*>|[^<>]*</)"))
                {
                    replacedItem = "%" + replacedItem + "%";
                }
                if (isButtonParam) 
                {
                    if (item.paraname.EqualsCI("qlink")) 
                    {
                        var schedulelinkKeyValue = staticobjects.GetDictionarykeyValueStringObject("%qLinkKey%");
                        item.value = schedulelinkKeyValue;
                    }
                }
                 
                else
                {
                    var valueParam = (!string.IsNullOrWhiteSpace(item.value) && !Regex.IsMatch(item.value, @"%([a-zA-Z0-9.])+%")) ?
                    item.value : DynamicExtensions.DynamicVariablesReplace(replacedItem, exObj, staticobjects);
                    if (string.IsNullOrWhiteSpace(valueParam)) 
                    {
                        valueParam = item.paraname;
                    }

                    item.value = valueParam;
                }
                
            }
        }
        private static void SetHeaderParamValues(Dictionary<string, object> exObj, Dictionary<string, object> staticobjects, List<WhatsappMessage.HeaderParameter> topitem) {
            foreach (var item in topitem) {
                string replacedItem = item.paraname;
                if (!Regex.IsMatch(replacedItem, @"%([a-zA-Z0-9.])+%(?![^<]*>|[^<>]*</)")) {
                    replacedItem = "%" + replacedItem + "%";
                }

                var valueParam = (!string.IsNullOrWhiteSpace(item.value) && !Regex.IsMatch(item.value, @"%([a-zA-Z0-9.])+%")) ?
                item.value : DynamicExtensions.DynamicVariablesReplace(replacedItem, exObj, staticobjects);
                if (string.IsNullOrWhiteSpace(valueParam)) {
                    valueParam = item.paraname;
                }

                item.value = valueParam;
            }
        }

    }
}
