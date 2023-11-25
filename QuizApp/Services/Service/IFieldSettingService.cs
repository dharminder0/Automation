using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service {
	public interface IFieldSettingService {
		ResultEnum Status { get; set; }
		string ErrorMessage { get; set; }
		List<FieldSyncSettingResponse> FieldSyncSetting();
	}
}