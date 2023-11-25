using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Common.Exceptions {
    public class LogInfoException : ApplicationException {
        public LogInfoException() {

        }

        public LogInfoException(string message):base(message) {
            
        }

        public LogInfoException(string message, Exception innerException):base(message, innerException) {

        }
    }
}
