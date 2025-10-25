using Microsoft.EntityFrameworkCore;
using SchrodingersBot.DB.DBO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SchrodingersBot.DB.Repositories
{
    public interface IDbRepository<T>
        where T : BaseEntity
    {
        public Task<T?> GetByIdAsync(int id);
        public Task<IEnumerable<T>> GetAllAsync();
        public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        public Task<T> CreateAsync(T entity);
        public Task UpdateAsync(T entity);
        public Task DeleteAsync(T entity);
        public Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    }
}
