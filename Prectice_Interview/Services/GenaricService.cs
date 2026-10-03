using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Prectice_Interview.Data;

namespace Prectice_Interview.Services
{
    public class GenaricService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _factory;

        public GenaricService(IDbContextFactory<ApplicationDbContext> factory) => _factory = factory;

        public async Task<T> AddAsync<T>(T entity) where T : class
        {
            await using var db = await _factory.CreateDbContextAsync();
            db.Set<T>().Add(entity);
            await db.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync<T>(T entity) where T : class
        {
            await using var db = await _factory.CreateDbContextAsync();
            db.Set<T>().Update(entity);
            await db.SaveChangesAsync();
            return entity;
        }

        public async Task DeleteAsync<T>(T entity) where T : class
        {
            await using var db = await _factory.CreateDbContextAsync();
            if (entity is CommonField cf)
            {
                db.Set<T>().Attach(entity);
                cf.IsDeleted = true;
                cf.IsActive = false;
                db.Entry(entity).State = EntityState.Modified;
            }
            else db.Set<T>().Remove(entity);
            await db.SaveChangesAsync();
        }

        public async Task<T?> GetByIdAsync<T>(int id) where T : class
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Set<T>().FindAsync(id);
        }

        public async Task<List<T>> GetAllAsync<T>() where T : class
        {
            await using var db = await _factory.CreateDbContextAsync();
            return await db.Set<T>().AsNoTracking().ToListAsync();
        }
    }
}
