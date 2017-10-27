using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Data.Entity.Validation;

namespace BillBoardsManagement.Repository
{
    public class Repository<T> : IDisposable, IRepository<T, int> where T : class
    {
        //The dendency for the DbContext specified the current class. 
        private BBMSEntities Context { get; set; }


        // *************************
        // *** Constructor ***
        // *************************
        public Repository(BBMSEntities context = null)
        {
            Context = context ?? new BBMSEntities();
        }


        // *************************
        // *** Disposable Methods ***
        // *************************
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }
            this.disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        // *************************
        // *** Sync Methods ***
        // *************************
        //Get all collections
        public IEnumerable<T> GetAll()
        {
            return Context.Set<T>().ToList();
        }

        //Get Specific collection based on id
        public T Get(int id)
        {
            return Context.Set<T>().Find(id);
        }
        public T GetByGuid(Guid id)
        {
            return Context.Set<T>().Find(id);
        }

        public IEnumerable<T> FindAll(Expression<Func<T, bool>> where)
        {
            return Context.Set<T>().Where(where).ToList();
        }

        //Create a new entity
        public T Post(T entity)
        {
            Context.Set<T>().Add(entity);
            Context.SaveChanges();
            return entity;
        }

        public bool PostAll(List<T> entity)
        {
            Context.Set<T>().AddRange(entity);
            try
            {

                return Context.SaveChanges() > 0;
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                throw;
            }
        }

        public bool SaveChanges()
        {
            return Context.SaveChanges() > 0;

        }

        //Update exisitng entity
        public T Put(int id, T entity)
        {
            if (entity == null)
                return null;

            T existing = Context.Set<T>().Find(id);
            if (existing != null)
            {
                Context.Entry(existing).CurrentValues.SetValues(entity);
                Context.SaveChanges();
            }
            return existing;
        }

        //Delete Exisitng entity
        public int Delete(int id)
        {
            var existing = Context.Set<T>().Find(id);
            if (existing != null)
            {
                Context.Set<T>().Remove(existing);
                return Context.SaveChanges();
            }
            return 0;
        } 

        //Count of the collection
        public int Count()
        {
            return Context.Set<T>().Count();
        }

        public IQueryable<T> GetAllQueriable()
        {
            return Context.Set<T>();
        }

        //get all, including select clause, where clause, order by clause, and includes
        //usage: var s = repository.GetAll(i => new { i.Name }, i => i.Name.Contains('John'), i => i.Name, false, i => i.NavigationProperty
        public IEnumerable<TReturn> GetAll<TReturn, TOrderKey>(Expression<Func<T, TReturn>> selectExp,
                                                               Expression<Func<T, bool>> whereExp,
                                                               Expression<Func<T, TOrderKey>> orderbyExp,
                                                               bool descending,
                                                               params Expression<Func<T, object>>[] includeExps)
        {
            var query = Context.Set<T>().Where(whereExp);
            query = !descending ? query.OrderBy(orderbyExp) : query.OrderByDescending(orderbyExp);
            if (includeExps != null)
                query = includeExps.Aggregate(query, (current, exp) => current.Include(exp));

            return query.Select(selectExp).ToList();
        }

        // *************************
        // *** Async Methods ***
        // *************************
        //Get all collections
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            //return Context.Set<T>().ToList();
            return await Context.Set<T>().ToListAsync();
        }

        //Get Specific collection based on id
        public async Task<T> GetAsync(int id)
        {
            return await Context.Set<T>().FindAsync(id);
        }

        public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> where)
        {
            return await Context.Set<T>().Where(where).ToListAsync();
        }

        //Create a new entity
        public async Task<T> PostAsync(T entity)
        {
            Context.Set<T>().Add(entity);
            await Context.SaveChangesAsync();
            return entity;
        }

        //Update exisitng entity
        public async Task<T> PutAsync(int id, T entity)
        {
            if (entity == null)
                return null;

            T existing = await Context.Set<T>().FindAsync(id);
            if (existing != null)
            {
                Context.Entry(existing).CurrentValues.SetValues(entity);
                //existing.Title = entity.Title;
                //existing.UpdatedBy = entity.UpdatedBy;
                //existing.UpdatedAt = DateTime.Now;

                //await Context.SaveChangesAsync(); 
            }
            return existing;
        }

        //Delete Exisitng entity
        public async Task DeleteAsync(int id)
        {
            var existing = await Context.Set<T>().FindAsync(id);
            if (existing != null)
            {
                Context.Set<T>().Remove(existing);
                await Context.SaveChangesAsync();
            }
        }

        //Count of the collection
        public Task<int> CountAsync()
        {
            return Context.Set<T>().CountAsync();
        }

    }
}