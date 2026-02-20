using System.Threading.Tasks;
using SimpleApi.Core.UnitOfWorks;
using SimpleApi.Models;

namespace SimpleApi.Data.UnitOfWorks
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApiDbContext _context;

        public UnitOfWork(ApiDbContext context)
        {
            _context = context;
        }

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public void Commit() => _context.SaveChanges();

        public async ValueTask DisposeAsync() => await _context.DisposeAsync();
    }
}
