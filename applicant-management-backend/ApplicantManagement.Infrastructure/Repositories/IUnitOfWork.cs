using System;
using System.Threading.Tasks;

namespace ApplicantManagement.Infrastructure.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        Task<int> SaveChangesAsync();
        bool HasActiveTransaction { get; }
    }
}