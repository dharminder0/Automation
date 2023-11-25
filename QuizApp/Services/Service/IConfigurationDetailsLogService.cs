using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Services.Service
{
   public interface IConfigurationDetailsLogService
   {
        //void ClearOldConfigurationDetails(int quizAttemptId);
        void ClearOldConfigurationByQuizAttemptId(int attemptId);
   }
}
