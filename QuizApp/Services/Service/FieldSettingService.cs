using Newtonsoft.Json;
using QuizApp.Db;
using QuizApp.Helpers;
using QuizApp.RepositoryPattern;
using QuizApp.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.Services.Service {
	public class FieldSettingService : IFieldSettingService {

		private ResultEnum status = ResultEnum.Ok;
		private string errormessage = string.Empty;
		public ResultEnum Status { get { return status; } set { status = value; } }
		public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

		public FieldSettingService() {

		}

		public List<FieldSyncSettingResponse> FieldSyncSetting() {
			List<FieldSyncSettingResponse> fieldSyncObj = new List<FieldSyncSettingResponse>();
			using (var UOWObj = new AutomationUnitOfWork()) {
				var fieldSyncSettingObj = UOWObj.FieldSyncSettingRepository.Get().ToList();
				if(fieldSyncSettingObj != null && fieldSyncSettingObj.Any()) {
					foreach(var item in fieldSyncSettingObj) {
						var fieldSyncSetting = new FieldSyncSettingResponse {
							Id = item.Id,
							FieldType = item.FieldType,
							FieldOptionName = item.FieldOptionName,
							FieldOptionTitle = item.FieldOptionTitle,
							Fields = !string.IsNullOrWhiteSpace(item.FieldFormula) ? JsonConvert.DeserializeObject<List<string>>(item.FieldFormula) : null
						};
						fieldSyncObj.Add(fieldSyncSetting);

					}
				}
			}
			return fieldSyncObj;
		}
	}
}