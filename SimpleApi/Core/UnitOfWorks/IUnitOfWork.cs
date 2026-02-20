using System;
using System.Threading.Tasks;

namespace SimpleApi.Core.UnitOfWorks
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task<int> CommitAsync();
        void Commit();
    }
}
