using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.Entity;
using System.Linq.Expressions;
using QuizApp.Db;

namespace QuizApp.RepositoryPattern
{
    public class GenericRepository<TEntity> where TEntity : class
    {
        internal QuizAppDBContext context;
        internal DbContextTransaction transaction;
        internal DbSet<TEntity> dbSet;

        public GenericRepository(QuizAppDBContext context)
        {
            this.context = context;
            this.dbSet = context.Set<TEntity>();
        }

        public virtual IEnumerable<TEntity> Get(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public virtual IQueryable<TEntity> GetQueryable(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query);
            }
            else
            {
                return query;
            }
        }

        public virtual int GetCount(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).Count();
            }
            else
            {
                return query.Count();
            }
        }

        public virtual IEnumerable<TEntity> GetNotTracked(Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).AsNoTracking().ToList();
            }
            else
            {
                return query.AsNoTracking().ToList();
            }
        }

        public TEntity GetFirstRecord(Expression<Func<TEntity, bool>> filter = null, params Expression<Func<TEntity, object>>[] includes)
        {
            IQueryable<TEntity> set = dbSet;

            if (includes != null)
            {
                foreach (var include in includes)
                {
                    set.Include(include);
                }
            }

            return (filter == null) ?
                   set.FirstOrDefault() :
                   set.FirstOrDefault(filter);
        }

        public virtual TEntity GetByID(object id)
        {
            return dbSet.Find(id);
        }

        public virtual void Insert(TEntity entity)
        {
            dbSet.Add(entity);
        }

        public virtual void BulKInsert(IEnumerable<TEntity> entity)
        {
            dbSet.AddRange(entity);
        }

        public virtual void Delete(object id)
        {
            TEntity entityToDelete = dbSet.Find(id);
            Delete(entityToDelete);
        }

        public virtual void Delete(TEntity entityToDelete)
        {
            if (context.Entry(entityToDelete).State == System.Data.Entity.EntityState.Detached)
            {
                dbSet.Attach(entityToDelete);
            }
            dbSet.Remove(entityToDelete);
        }

        public virtual void DeleteRange(string query, object[] parameters = null)
        {
            if (parameters == null)
            {
                parameters = new object[] { };
            }
            context.Database.ExecuteSqlCommand(query, parameters);
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            dbSet.Attach(entityToUpdate);
            context.Entry(entityToUpdate).State = System.Data.Entity.EntityState.Modified;
        }

        public virtual IEnumerable<TEntity> GetWithPagination(out int totalCount, int PageNo, int PageSize, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "")
        {
            IQueryable<TEntity> query = dbSet;
            totalCount = 0;

            if (filter != null)
            {
                query = query.Where(filter);
                totalCount = query.Count();
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = query.Include(includeProperty);
            }

            if (orderBy != null)
            {
                return orderBy(query).Skip(PageSize * (PageNo - 1)).Take(PageSize).ToList();
            }
            else
            {
                return query.Skip(PageSize * (PageNo - 1)).Take(PageSize).ToList();
            }
        }

        public IEnumerable<TType> GetSelectedColoumn<TType>(Expression<Func<TEntity, TType>> select, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "") where TType : class
        {
            ICollection<TType> query;

            if (filter != null)
            {
                query = dbSet.Where(filter).Select(select).ToList();
            }
            else
            {
                query = dbSet.Select(select).ToList();
            }

            foreach (var includeProperty in includeProperties.Split
                (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                query = ((IQueryable<TType>)query).Include(includeProperty).ToList();
            }

            if (orderBy != null)
            {
                return (ICollection<TType>)orderBy((IQueryable<TEntity>)query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }

        public IEnumerable<TType> GetSelectedColoumnV2<TType>(Expression<Func<TEntity, TType>> select, Expression<Func<TEntity, bool>> filter = null, Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null, string includeProperties = "", int? pageSize = null) where TType : class
        {
            ICollection<TType> query;
            IQueryable<TType> querystring;
            if (filter != null)
            {
                querystring = dbSet.Where(filter).Select(select);
            }
            else
            {
                querystring = dbSet.Select(select);
            }
            if (pageSize.HasValue && pageSize.Value > 0)
            {
                querystring = querystring.Take(pageSize.Value);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split
                    (new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    querystring = querystring.Include(includeProperty);
                }
            }
            query = querystring.ToList();


            if (orderBy != null)
            {
                return (ICollection<TType>)orderBy((IQueryable<TEntity>)query).ToList();
            }
            else
            {
                return query.ToList();
            }
        }
    }
}