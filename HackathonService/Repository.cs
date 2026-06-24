using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace HackathonService
{
    public interface IRepository<T> where T : class
    {
        
        Task<T> GetByIdAsync(int id);
        Task<List<T>> GetAllAsync();

        Task<List<T>> FindAsync( Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T  entity);
        Task<int> CountAsync( Expression<Func<T, bool>>  predicate = null);
    }
    public class Repository<T> : IRepository<T> where T  : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;
        public Repository(AppDbContext  context)
        {
            _context = context;
            _dbSet =  context.Set<T>();
        }
        public async  Task<T> GetByIdAsync(int id)
        {
            return  await _dbSet.FindAsync(id) ;
        }
        public async Task<List<T>>  GetAllAsync()
        {
            return await  _dbSet.ToListAsync();
        }
        public async Task <List<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await  _dbSet.Where(predicate).ToListAsync();
        }


        public  async Task<T> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }
        public async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync();
        }



        public async Task DeleteAsync(T entity)
        {
            _dbSet.Remove(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<int> CountAsync(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
                return await _dbSet.CountAsync();
            return await _dbSet.CountAsync(predicate);
        }
    }
}