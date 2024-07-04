using Microsoft.EntityFrameworkCore;

using NuovoAutoServer.Model;
using NuovoAutoServer.Model.Constants;
using NuovoAutoServer.Repository.DBContext;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NuovoAutoServer.Repository.Repository
{
    public class CosmosDbEFRepository : IGenericRepository<CosmosDBContext> , IDisposable
    {
        public CosmosDbEFRepository(CosmosDBContext dataContext)
        {
            context = dataContext;
        }

        public CosmosDBContext context { get; private set; }

        public CosmosDBContext DbContext()
        {
            return context;
        }

        #region Implement IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    context.Dispose();
                }

                _disposed = true;

            }
        }

        public IQueryable<TEntity> Get<TEntity>(string[]? includes = null) where TEntity : DomainModelBase
        {
            var query = context.Set<TEntity>().AsQueryable().Where(x => x.RecordStatus == RecordStatusConstants.Active);
            return query;
        }

        public IQueryable<TEntity> Query<TEntity>(Expression<Func<TEntity, bool>> predicate, string[]? includes = null) where TEntity : DomainModelBase
        {
            var query = context.Set<TEntity>().AsQueryable().Where(x => x.RecordStatus == RecordStatusConstants.Active);
            query = query.Where(predicate);
            return query;
        }

        public async Task<TEntity> AddAsync<TEntity>(TEntity item) where TEntity : class
        {
            if (item != null)
            {
                await context.Set<TEntity>().AddAsync(item);
                await context.SaveChangesAsync();
            }
            return item;
        }

        public async Task<IEnumerable<TEntity>> AddRangeAsync<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            await context.Set<TEntity>().AddRangeAsync(items);
            await context.SaveChangesAsync();
            return items;
        }

        public async Task<TEntity> UpdateAsync<TEntity>(TEntity item) where TEntity : class
        {
            if (item != null)
            {
                var existingEntity = context.Set<TEntity>().Find(context.Entry(item).Property("Id").CurrentValue, context.Entry(item).Property("PartitionKey").CurrentValue);

                if (existingEntity != null)
                {
                    context.Entry(existingEntity).State = EntityState.Detached;
                }

                (item as DomainModelBase).CreatedDateTime = (existingEntity as DomainModelBase).CreatedDateTime;

                context.Entry(item).State = EntityState.Modified;
                await context.SaveChangesAsync();
            }
            return item;
        }

        public Task<TEntity> UpdateEntryAsync<TEntity>(TEntity item, IDictionary<string, object> fields) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task UpdateEntryRangeAsync<TEntity>(IDictionary<TEntity, IDictionary<string, object>> item) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<TEntity>> UpdateRangeAsync<TEntity>(IEnumerable<TEntity> items) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public async Task<bool> RemoveAsync<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity == null)
                return false;

            context.Set<TEntity>().Remove(entity);
            await context.SaveChangesAsync();
            return true;
        }

        public Task<bool> RemoveRangeAsync<TEntity>(params TEntity[] entities) where TEntity : class
        {
            throw new NotImplementedException();
        }
        #endregion // Implement IDisposable

    }
}
