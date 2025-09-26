using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ApplicantManagement.Domain.Repositories
{
    public interface IRepository<T> where T : class
    {
        Task<T> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task SaveChangesAsync();
        
        // New methods for pagination
        Task<int> CountAsync(Expression<Func<T, bool>> predicate);
        Task<List<T>> GetPagedAsync(
            Expression<Func<T, bool>> predicate,
            int pageNumber,
            int pageSize,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null);
    }
}