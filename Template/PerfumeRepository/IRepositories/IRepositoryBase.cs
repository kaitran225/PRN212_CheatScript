using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace PerfumeRepository.IRepositories
{
    public interface IRepositoryBase<T> where T : class
    {
        // Get all entities
        IEnumerable<T> GetAll();
        
        // Get entities with filter
        IEnumerable<T> Find(Expression<Func<T, bool>> expression);
        
        // Add entity
        void Add(T entity);
        
        // Update entity
        void Update(T entity);
        
        // Delete entity
        void Delete(T entity);
        
        // Get entity by ID
        T GetById(object id);
        
        // Count entities with filter
        int Count(Expression<Func<T, bool>> expression);
    }
} 