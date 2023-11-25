using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Core.Common.Extensions
{
    public static class DynamicExtensions
    {
        public static string DynamicVariablesReplace(string message, Dictionary<string, object> contactObjs, Dictionary<string, object> staticobjects, string msgVariables = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    return null;
                }

                var list = new List<string>();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (!string.IsNullOrWhiteSpace(msgVariables))
                    {
                        list = JsonConvert.DeserializeObject<List<string>>(msgVariables);
                    }
                    else
                    {
                        var m1 = Regex.Matches(message, @"%([a-zA-Z0-9.])+%");

                        foreach (Match match in m1)
                        {
                            list.Add(match.Value.ToLower());
                        }
                    }
                }

                message = Replacevariables(message, contactObjs, staticobjects, msgVariables, list);

                list = new List<string>();
                if (!string.IsNullOrWhiteSpace(message))
                {
                    var m1 = Regex.Matches(message, @"%([a-zA-Z0-9.])+%");

                    foreach (Match match in m1)
                    {
                        list.Add(match.Value.ToLower());
                    }
                }
                if (list != null && list.Any())
                {
                    message = Replacevariables(message, contactObjs, staticobjects, null, list);
                }

                return message;
            }
            catch (Exception ex)
            {

            }

            return null;
        }

        private static string Replacevariables(string message, Dictionary<string, object> contactObjs, Dictionary<string, object> staticobjects, string msgVariables, List<string> list)
        {
            var exObjpair = contactObjs.ToDictionary(k => "%" + k.Key.ToLower() + "%", k => k.Value);
            exObjpair.AddDictionarykey("%fname%", exObjpair.GetDictionarykeyValueStringObject("%firstname%"));
            exObjpair.AddDictionarykey("%lname%", exObjpair.GetDictionarykeyValueStringObject("%lastname%"));
            exObjpair.AddDictionarykey("%phone%", exObjpair.GetDictionarykeyValueStringObject("%telephone%"));
            exObjpair.AddDictionarykey("%email%", exObjpair.GetDictionarykeyValueStringObject("%email%"));

            foreach (var item in list)
            {
                if (exObjpair.CheckDictionarykeyExistStringObject(item))
                {
                    // message = message.Replace(item, Convert.ToString(exObjpair[item]));
                    // message = Regex.Replace(message, item, exObjpair.GetDictionarykeyValueStringObject(item), RegexOptions.IgnoreCase);
                    message = message.RegexReplace(item, exObjpair.GetDictionarykeyValueStringObject(item));
                    //message = Regex.Replace(message, item, Convert.ToString(exObjpair[item]), RegexOptions.IgnoreCase);
                }

                else if (staticobjects != null && staticobjects.CheckDictionarykeyExistStringObject(item))
                {
                    //message = message.Replace(item, Convert.ToString(staticobjects[item]));
                    //message = Regex.Replace(message, item, staticobjects.GetDictionarykeyValueStringObject(item), RegexOptions.IgnoreCase);
                    message = message.RegexReplace(item, staticobjects.GetDictionarykeyValueStringObject(item));
                }

                else if (!string.IsNullOrWhiteSpace(msgVariables))
                {
                    //message = message.Replace(item, string.Empty);
                    message = Regex.Replace(message, item, string.Empty, RegexOptions.IgnoreCase);
                }
            }

            return message;
        }
    }
}
