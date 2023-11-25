using Core.Common.Extensions;
using Newtonsoft.Json;
using NLog;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;
using static QuizApp.Helpers.Models;
using static QuizApp.Response.OWCLeadUserResponse;

namespace QuizApp.Helpers
{
    public class PrerenderHelper
    {
        public static NLog.Logger Logger = LogManager.GetCurrentClassLogger();
        public static async Task RecachePrerenderedPage(string pageURL, int workpackageid)
        {
          
            bool status = false;
            try
            {
                dynamic requestBody = new ExpandoObject();
                requestBody.prerenderToken = ConfigurationManager.AppSettings["PrerenderApiToken"];
                requestBody.url = pageURL;

                var serviceUrl = ConfigurationManager.AppSettings["PrerenderApiUrl"];

                try
                {
                    using (var client = new HttpClient())
                    {
                        client.Timeout = new TimeSpan(0, 20, 0);

                        HttpResponseMessage result = null;
                        await client.PostAsync(serviceUrl, requestBody).Result;
                    }
                }
                catch (Exception ex)
                {

                }
            }
            catch (Exception ex)
            {
            }

         
        }
    }
}