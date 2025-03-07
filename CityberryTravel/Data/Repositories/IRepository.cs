using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CityberryTravel.Data.Repositories
{
    public interface IRepository<T> where T : class
    {
        // Get Methods
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        
        // Add Methods
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        
        // Update Methods
        void Update(T entity);
        
        // Remove Methods
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
} 