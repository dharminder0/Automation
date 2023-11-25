using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace QuizApp.RepositoryPattern
{
    //public interface IGenericRepository<TEntity> where TEntity : class
    //{
    //    IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "");
    //    IEnumerable<TEntity> GetWithPagination(out int totalCount, int PageNo, int PageSize, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "");
    //    IEnumerable<TType> GetSelectedColoumn<TType>(Expression<Func<TEntity, TType>> select, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "") where TType : class;
    //    void DeleteRange(string query, object[] parameters =null);
    //    TEntity GetByID(object id);
    //    void Insert(TEntity entity);
    //    void Delete(object id);
    //    void Delete(TEntity entityToDelete);
    //    void Update(TEntity entityToUpdate);
    //    void BulKInsert(IEnumerable<TEntity> entity);
    //}
}
