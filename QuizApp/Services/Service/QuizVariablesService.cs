using QuizApp.Helpers;
using QuizApp.RepositoryExtensions;
using QuizApp.RepositoryPattern;
using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QuizApp.Services.Service
{
    public partial class QuizVariablesService : IQuizVariablesService
    {
        private ResultEnum status = ResultEnum.Ok;
        private string errormessage = string.Empty;

        public ResultEnum Status { get { return status; } set { status = value; } }
        public string ErrorMessage { get { return errormessage; } set { errormessage = value; } }

        public List<string> GetQuizVariables(int quizDetailsId, int objectid, int objectType)
        {
            List<string> Variables = new List<string>();
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizVariablesObj = UOWObj.QuizVariablesRepository.Get(v => v.QuizDetailsId == quizDetailsId && v.ObjectId == objectid && v.ObjectTypes == objectType).FirstOrDefault();

                    if (quizVariablesObj != null)
                    {
                        Variables = quizVariablesObj.Variables != null ? quizVariablesObj.Variables.Split(',').ToList() : new List<string>();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Variables;
        }

        //public void UpdateQuizVariables(QuizVariableModel quizVariable)
        //{
        //    try
        //    {
        //        using (var UOWObj = new AutomationUnitOfWork())
        //        {
        //            var objectVariableDetails = UOWObj.QuizVariablesRepository.GetQuizVariablesByObjectId(quizVariable.ObjectId, quizVariable.ObjectTypes);
        //            if (objectVariableDetails != null && objectVariableDetails.Any())
        //            {
        //                foreach (var item in objectVariableDetails)
        //                {
        //                    item.ObjectTypes = quizVariable.ObjectTypes;
        //                    item.Variables = quizVariable.Variables;
        //                    item.ObjectId = quizVariable.ObjectId;
        //                    item.QuizDetailsId = quizVariable.QuizDetailsId;
        //                    item.CompanyId = quizVariable.CompanyId;

        //                    UOWObj.QuizVariablesRepository.Update(item);
        //                }
        //                UOWObj.Save();
        //            }
        //            else
        //            {
        //                Status = ResultEnum.Error;
        //                ErrorMessage = "QuizVariables is required";
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Status = ResultEnum.Error;
        //        ErrorMessage = ex.Message;
        //        throw ex;
        //    }
        //}

        public QuizVariableModel AddQuizVariables(QuizVariableModel quizVariable)
        {
            try
            {
                using (var UOWObj = new AutomationUnitOfWork())
                {
                    var quizVariables = UOWObj.QuizVariablesRepository.GetQuizVariablesByObjectId(quizVariable.ObjectId, quizVariable.ObjectTypes, quizVariable.QuizDetailsId).FirstOrDefault();
                    if (quizVariables != null )
                    {
                        quizVariables.ObjectTypes = quizVariable.ObjectTypes;
                        quizVariables.Variables = quizVariable.Variables;
                        quizVariables.ObjectId = quizVariable.ObjectId;
                        quizVariables.QuizDetailsId = quizVariable.QuizDetailsId;
                        quizVariables.CompanyId = quizVariable.CompanyId;

                        UOWObj.QuizVariablesRepository.Update(quizVariables);
                        UOWObj.Save();
                    }
                    else
                    {
                            Db.QuizVariables obj = new Db.QuizVariables
                            {
                                ObjectTypes = quizVariable.ObjectTypes,
                                Variables = quizVariable.Variables,
                                ObjectId = quizVariable.ObjectId,
                                QuizDetailsId = quizVariable.QuizDetailsId,
                                CompanyId = quizVariable.CompanyId
                            };
                            UOWObj.QuizVariablesRepository.Insert(obj);
                            UOWObj.Save();
                            quizVariable.Id = obj.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                Status = ResultEnum.Error;
                ErrorMessage = ex.Message;
                throw ex;
            }
            return quizVariable;
        }
    }
}