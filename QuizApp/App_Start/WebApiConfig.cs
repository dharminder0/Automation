using QuizApp.Controllers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Cors;

namespace QuizApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            var cors = new EnableCorsAttribute("*", "*", "*") { SupportsCredentials = true };
            config.EnableCors(cors);

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            UnityConfig.RegisterComponents();
            var useSingletonContext = bool.Parse(string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["UseSingletonContext"]) ? "true" : ConfigurationManager.AppSettings["UseSingletonContext"]);
            config.Filters.Add(new DbContextTrackingFilter(useSingletonContext));

            config.Formatters.XmlFormatter.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("multipart/form-data"));

            var loggingFilterEnabled = bool.Parse(ConfigurationManager.AppSettings["ApiLoggingFilterEnabled"]);
            if (loggingFilterEnabled)
                config.Filters.Add(new LoggingFilter());
        }
    }
}
