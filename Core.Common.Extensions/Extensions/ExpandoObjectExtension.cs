using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace Core.Common.Extensions
{
    public static class ExpandoObjectExtension
    {

        //public static void AddORUpdateProperty(this ExpandoObject expando, string propertyName, object propertyValue)
        //{
        //    // ExpandoObject supports IDictionary so we can extend it like this
        //    var expandoDict = expando as IDictionary<string, object>;
        //    if (expandoDict.ContainsKey(propertyName))
        //        expandoDict[propertyName] = propertyValue;
        //    else
        //        expandoDict.Add(propertyName, propertyValue);
        //}
       
        //public static object GetExpandoProperty(this ExpandoObject expando, string propertyName)
        //{
        //    // ExpandoObject supports IDictionary so we can extend it like this
        //    //var expandoDict = expando as IDictionary<string, object>;
        //    //if (expandoDict.ContainsKey(propertyName))
        //    //    return expandoDict[propertyName];
        //     return null;
        //}

        //public static string FirstOrDefault(this ExpandoObject eo, string key)
        //{
         
        //    object r = eo.FirstOrDefault(x => x.Key == key).Value;
        //    return (r is string) ? (string)r : default(string);
        //}
    }
}
