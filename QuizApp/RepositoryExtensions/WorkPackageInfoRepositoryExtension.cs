using QuizApp.Db;
using QuizApp.RepositoryPattern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryExtensions
{
    public static class WorkPackageInfoRepository
    {
        public static IEnumerable<WorkPackageInfo> GetWorkpackageById(this GenericRepository<WorkPackageInfo> repository, int id)
        {
            return repository.Get(r => r.Id == id);
        }
    }
}