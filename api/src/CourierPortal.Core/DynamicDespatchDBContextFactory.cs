using System;
using System.Linq;
using CourierPortal.Core.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Serilog;

namespace CourierPortal.Core
{
    public class DynamicDespatchDbContextFactory(
        IOptions<DbContextOptions<DespatchContext>> options,
        IConnectionStringManager connectionStringManager,
        IHttpContextAccessor contextAccessor)
        : IDbContextFactory<DespatchContext>
    {
        private readonly DbContextOptions<DespatchContext> _options = options.Value;

        public  DespatchContext CreateDbContext()
        {
            var tenantId = contextAccessor.HttpContext?.User.Claims.FirstOrDefault(x => x.Type == "CurrentTenantID")?.Value;
            var connectionString = connectionStringManager.GetConnectionStringAsync($"{tenantId}-CourierManager-Connection").GetAwaiter().GetResult();
        
            if (string.IsNullOrEmpty(connectionString))
            {
                Log.Error("Connection string is not set");
                throw new InvalidOperationException("Connection string is not set");
            }

            var optionsBuilder = new DbContextOptionsBuilder<DespatchContext>(_options);
            optionsBuilder.UseSqlServer(connectionString);

            Log.Information("Creating DespatchDbContext with connection string");
            return new DespatchContext(optionsBuilder.Options);
        }
    }
}
