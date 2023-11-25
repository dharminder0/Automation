using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace Core.Common.Extensions
{
    public static class ObjectExtensions {
        public static JArray ToJArray(this object obj, bool camelCaseNames = true) {
            var serializer = new JsonSerializer();
            if (camelCaseNames) {
                serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }
            return JArray.FromObject(obj, serializer);
        }

        public static JObject ToJObject(this object obj, bool camelCaseNames = true) {
            var serializer = new JsonSerializer();
            if (camelCaseNames) {
                serializer.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }
            return JObject.FromObject(obj, serializer);
        }        

        public static T1 Map<T1>(this object src, params string[] ignoredProperties) {
            var type = typeof(T1);
            T1 dest = (T1)Activator.CreateInstance(type);
            var destProperties = TypeDescriptor.GetProperties(typeof(T1));
            foreach (PropertyDescriptor item in TypeDescriptor.GetProperties(src)) {
                if ((ignoredProperties != null && ignoredProperties.Contains(item.Name)) || type.GetProperty(item.Name) == null)
                    continue;
                var val = item.GetValue(src);
                destProperties[item.Name].SetValue(dest, val);
            }
            return dest;
        }

        public static void CopyProperties(this object srcObj, object destObj) {
            foreach (var prop in srcObj.GetType().GetProperties()) {
                prop.SetValue(destObj, prop.GetValue(srcObj));
            }
        }

        public static string ToXml<T>(this T src) {
            XmlSerializer xsSubmit = new XmlSerializer(typeof(T));
            var xml = "";
            using (var sww = new StringWriter()) {
                using (XmlWriter writer = XmlWriter.Create(sww)) {
                    xsSubmit.Serialize(writer, src);
                    xml = sww.ToString();
                }
            }
            return xml;
        }

        public static dynamic ToDynamic(this object obj, params string[] filteredProps) {
            Dictionary<string,object> dynObj = new Dictionary<string, object>();
            foreach (var prop in obj.GetType().GetProperties()) {
                if(filteredProps != null && filteredProps.Any()) {
                    if (filteredProps.Contains(prop.Name))
                        dynObj.Add(prop.Name, prop.GetValue(obj));
                }
                else
                    dynObj.Add(prop.Name, prop.GetValue(obj));
            }
            return dynObj;
        }
    }
}
