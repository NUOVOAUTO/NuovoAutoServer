using NuovoAutoServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Repository.Repository
{
    public interface IGenericRepository<T>
    {
        T DbContext();

        IQueryable<TEntity> Get<TEntity>(string[]? includes = null) where TEntity : DomainModelBase;

        IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> predicate, string[]? includes = null) where TEntity : DomainModelBase;

        Task<TEntity> AddAsync<TEntity>(TEntity item) where TEntity : class;

        Task<IEnumerable<TEntity>> AddRangeAsync<TEntity>(IEnumerable<TEntity> items) where TEntity : class;

        Task<TEntity> UpdateAsync<TEntity>(TEntity item) where TEntity : class;
        Task<TEntity> UpdateEntryAsync<TEntity>(TEntity item, IDictionary<string, object> fields) where TEntity : class;
        Task UpdateEntryRangeAsync<TEntity>(IDictionary<TEntity, IDictionary<string, object>> item) where TEntity : class;

        Task<IEnumerable<TEntity>> UpdateRangeAsync<TEntity>(IEnumerable<TEntity> items) where TEntity : class;

        Task<bool> RemoveAsync<TEntity>(TEntity entity) where TEntity : class;
        Task<bool> RemoveRangeAsync<TEntity>(params TEntity[] entities) where TEntity : class;

    }
}
