using ApplicantManagement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace ApplicantManagement.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UnitOfWork> _logger;
        private IDbContextTransaction _currentTransaction;

        public UnitOfWork(ApplicationDbContext context, ILogger<UnitOfWork> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public bool HasActiveTransaction => _currentTransaction != null;

        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already active.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
            _logger.LogInformation("Database transaction started");
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to commit.");
            }

            try
            {
                await _context.SaveChangesAsync();
                await _currentTransaction.CommitAsync();
                _logger.LogInformation("Database transaction committed successfully");
            }
            catch (Exception ex)
            {
                await RollbackTransactionAsync();
                _logger.LogError(ex, "Error committing transaction");
                throw;
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            try
            {
                await _currentTransaction.RollbackAsync();
                _logger.LogWarning("Database transaction rolled back");
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error occurred");
                // If there's an active transaction, it might be compromised
                if (_currentTransaction != null)
                {
                    _logger.LogWarning("Transaction may be compromised due to DbUpdateException");
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during save changes");
                // If there's an active transaction, it might be compromised
                if (_currentTransaction != null)
                {
                    _logger.LogWarning("Transaction may be compromised due to unexpected exception");
                }
                throw;
            }
        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}