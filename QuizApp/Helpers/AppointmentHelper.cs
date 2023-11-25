using Core.Common.Extensions;
using Newtonsoft.Json;
using QuizApp.Request;
using QuizApp.Response;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using static QuizApp.Response.OWCLeadUserResponse;

namespace QuizApp.Helpers {
    public class AppointmentHelper {
        public static string GetResponse(string requestTypeURL, string authorization, string requestType = "GET", string requestdata = "") {
            string json = string.Empty;
            string url = requestTypeURL;

            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.TryAddWithoutValidation("ApiSecret", authorization);
            //client.DefaultRequestHeaders.Add("Content-Type", "application/json");

            //HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            //request.Headers.Add("ApiSecret", authorization);

            //request.Method = requestType;
            //request.ContentType = "application/json";
            //request.MediaType = "application/json";
            //request.Accept = "application/json";
            switch (requestType) {
                case "GET":
                    var result = client.GetAsync(url).Result.Content;
                    json = result.ReadAsStringAsync().Result;
                    break;

                case "POST":
                    object obj = string.IsNullOrWhiteSpace(requestdata) ? null : JsonConvert.DeserializeObject(requestdata);
                    var resultPost = client.PostAsJsonAsync(url, obj).Result.Content;
                    json = resultPost.ReadAsStringAsync().Result;
                    break;

                default:
                    break;
            }


            //if (request.Method == "POST")
            //{
            //    var data = System.Text.Encoding.ASCII.GetBytes(requestdata);

            //    request.ContentLength = data.Length;

            //    using (var stream = request.GetRequestStream())
            //    {
            //        stream.Write(data, 0, data.Length);
            //    }
            //}

            //using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            //using (Stream stream = response.GetResponseStream())
            //using (StreamReader reader = new StreamReader(stream))
            //{
            //    json = reader.ReadToEnd();
            //}

            return json;
        }

        public static string PushWorkPackage(Helpers.Models.AppointmentWorkPackage model) {
            var appointmentPushWorkPackageURL = GlobalSettings.appointmentApiBaseUrl.ToString()+"/api/v1/Integration/PushWorkPackage";
            var appointmentAPIsecretKey = GlobalSettings.appointmentAPISecret.ToString();
            return GetResponse(appointmentPushWorkPackageURL, appointmentAPIsecretKey, "POST", JsonConvert.SerializeObject(model));
        }

        public static AppointmentTypeList GetAppointmentTypeDetailsList(string Id) {
            var appointmentGetAppointmentTypeDetailsListURL = GlobalSettings.appointmentApiBaseUrl.ToString() + "/api/v1/Appointment/GetAppointmentTypeDetailsList?Id={Id}";
            var appointmentAPIsecretKey = GlobalSettings.appointmentAPISecret.ToString();
            return JsonConvert.DeserializeObject<AppointmentTypeList>(GetResponse((appointmentGetAppointmentTypeDetailsListURL).Replace("{Id}", Id.ToString()), appointmentAPIsecretKey));
        }

    }
}