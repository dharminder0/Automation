using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Response {
	public class FieldSyncSettingResponse {

        public int Id { get; set; }
        public string FieldType { get; set; }

        public string FieldOptionName { get; set; }

        public string FieldOptionTitle { get; set; }

        public  List<string> Fields { get; set; }
    }

    public class FiledFormula {
        public string GETDATE { get; set; }
        public string CurrentDateTime { get; set; }
        public string GETUTCDATE { get; set; }
    }
}