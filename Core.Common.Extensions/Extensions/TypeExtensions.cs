using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Extensions {
    public static class TypeExtensions {
        public static Dictionary<string, string> GetConstants(this Type type) {
            Dictionary<string, string> constants = new Dictionary<string, string>();
            FieldInfo[] fieldInfos = type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (FieldInfo fi in fieldInfos)
                if (fi.IsLiteral && !fi.IsInitOnly)
                    constants.Add(fi.Name, fi.GetRawConstantValue().ToString());
            return constants;
        }
    }
}
