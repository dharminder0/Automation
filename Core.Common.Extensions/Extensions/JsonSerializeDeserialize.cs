using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Extensions
{
    public static class JsonSerializeDeserialize
    {
        public static string SerializeObjectWithoutNull(this object objectDetails)
        {
            if (objectDetails == null)
            {
                return null;
            }
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            var json = JsonConvert.SerializeObject(objectDetails, settings);
            return json;
        }
        public static string SerializeObjectWithNull(this object objectDetails)
        {
            if (objectDetails == null)
            {
                return null;
            }

            return JsonConvert.SerializeObject(objectDetails);
        }

        public static T DeserializeObjectWithoutNull<T>(this object objectDetails)
        {
            if(objectDetails == null)
            {
                return (T)objectDetails;
            }
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            return  JsonConvert.DeserializeObject<T>(objectDetails.ToString(), settings);
        }

        public static T DeserializeObjectWithNull<T>(this object objectDetails)
        {
            return JsonConvert.DeserializeObject<T>(objectDetails.ToString());
        }

    }
}
