using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace BillBoardsManagement.Repository

{
    public interface IRepository<T, in TKey> where T : class
    {
        //Sync Methods
        IEnumerable<T> GetAll();
        T Get(TKey id);
        IEnumerable<T> FindAll(Expression<Func<T, bool>> where);
        T Post(T entity);
        bool PostAll(List<T> entity);
        T Put(TKey id, T entity);
        int Delete(TKey id);
        int Count();
        IQueryable<T> GetAllQueriable();
        IEnumerable<TReturn> GetAll<TReturn, TOrderKey>(Expression<Func<T, TReturn>> selectExp,
                                    Expression<Func<T, bool>> whereExp,
                                    Expression<Func<T, TOrderKey>> orderbyExp,
                                    bool descending,
                                    params Expression<Func<T, object>>[] includeExps);
        // Async Methods
        Task<IEnumerable<T>> GetAllAsync();
        Task<T> GetAsync(TKey id);
        Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> where);
        Task<T> PostAsync(T entity);
        Task<T> PutAsync(TKey id, T entity);
        Task DeleteAsync(TKey id);
        Task<int> CountAsync();
    }
}
