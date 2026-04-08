using System;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Infrastructure.Repositories
{
    public class BaseRepository(IDbContextFactory<DespatchContext> contextFactory):IDisposable
    {
        private DespatchContext? _context;
        protected DespatchContext Context
        {
            get { return _context ??= contextFactory.CreateDbContext(); }
        }
        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
