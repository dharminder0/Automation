using QuizApp.Db;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QuizApp.RepositoryPattern
{
    //public class UnitOfWork : IDisposable
    //{
    //    private QuizAppDBContext context = new QuizAppDBContext();

    //    public IGenericRepository<Tbl_EntityType> GetRepositoryInstance<Tbl_EntityType>() where Tbl_EntityType : class
    //    {
    //        return new GenericRepository<Tbl_EntityType>(context);
    //    }

    //    public void Save()
    //    {
    //        try
    //        {
    //            context.SaveChanges();
    //        }
    //        catch
    //        {
    //            throw;
    //        }
    //    }

    //    private bool disposed = false;

    //    protected virtual void Dispose(bool disposing)
    //    {
    //        if (!this.disposed)
    //        {
    //            if (disposing)
    //            {
    //                context.Dispose();
    //            }
    //        }
    //        this.disposed = true;
    //    }

    //    public void Dispose()
    //    {
    //        Dispose(true);
    //        GC.SuppressFinalize(this);
    //    }
    //}
}
