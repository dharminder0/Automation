using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Extensions {
    public static class IntExtensions {

        /// <summary>
        /// Converts minutes to Dutch time format (1d, 20u, 23m)
        /// </summary>
        /// <param name="inputMnutes"></param>
        /// <returns></returns>
        public static string ToDutchTime(this int inputMnutes) {
            var output = "{0}d, {1}u, {2}m";
            var ts = new TimeSpan(0, inputMnutes, 0);
            return string.Format(output, ts.Days, ts.Hours, ts.Minutes);
        }

        public static int IfThen(this int input, int compareVal, int thenVal) {
            if (input == compareVal)
                return thenVal;
            return input;
        }

        public static int? IfThen(this int input, int compareVal, int? thenVal) {
            if (input == compareVal)
                return thenVal;
            return input;
        }

        public static int IfNull(this int? input, int thenVal) {
            if (input == null)
                return thenVal;
            return input.Value;
        }
    }
}
