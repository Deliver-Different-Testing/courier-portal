using System;
using CourierPortal.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CourierPortal.Core.Services.Portal
{
    public class PortalBaseService(IDbContextFactory<DespatchContext> contextFactory):IDisposable
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
