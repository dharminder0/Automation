using QuizApp.Services.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.Response
{
    public interface IResponse
    {
        IResponse MapEntityToResponse(Base obj);
    }
}
