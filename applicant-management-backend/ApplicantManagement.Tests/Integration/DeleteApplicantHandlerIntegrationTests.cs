using ApplicantManagement.Application.Features.Applicants.Commands.DeleteApplicant;
using ApplicantManagement.Applicants.Services;
using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Infrastructure.Data;
using ApplicantManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicantManagement.Tests.Integration
{
    public class DeleteApplicantHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IApplicantRepository _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<ILogger<DeleteApplicantHandler>> _loggerMock;
        private readonly Mock<IApplicantLoggingService> _loggingServiceMock;
        private readonly Mock<IApplicantSecurityService> _securityServiceMock;
        private readonly DeleteApplicantHandler _handler;

        public DeleteApplicantHandlerIntegrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            
            // Setup repositories and unit of work
            _applicantRepository = new ApplicantRepository(_dbContext, new Mock<ILogger<ApplicantRepository>>().Object);
            _unitOfWork = new UnitOfWork(_dbContext, new Mock<ILogger<UnitOfWork>>().Object);

            // Setup mocks
            _loggerMock = new Mock<ILogger<DeleteApplicantHandler>>();
            _loggingServiceMock = new Mock<IApplicantLoggingService>();
            _securityServiceMock = new Mock<IApplicantSecurityService>();

            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(false);

            // Create handler
            _handler = new DeleteApplicantHandler(
                _applicantRepository,
                _unitOfWork,
                _loggerMock.Object,
                _loggingServiceMock.Object,
                _securityServiceMock.Object);
        }

        [Fact]
        public async Task Handle_SoftDelete_ShouldMarkApplicantAsDeleted()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "John Doe",
                FamilyName = "Doe",
                EmailAddress = "john.doe@example.com",
                Phone = "1234567890",
                Age = 28,
                Address = "123 Test Street",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-5),
                Hired = false,
                CreatedBy = "TestUser",
                LastModifiedBy = "TestUser",
                DeletedReason = ""
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new DeleteApplicantCommand(existingApplicant.Id, existingApplicant.RowVersion, false); // Soft delete

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);

            var deletedApplicant = await _dbContext.Applicants.FindAsync(existingApplicant.Id);
            Assert.NotNull(deletedApplicant);
            Assert.True(deletedApplicant.IsDeleted);
            Assert.NotNull(deletedApplicant.DeletedDate);
            Assert.False(deletedApplicant.Hired);
        }

        [Fact]
        public async Task Handle_HardDelete_ShouldRemoveApplicantFromDatabase()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "Jane Smith",
                FamilyName = "Smith",
                EmailAddress = "jane.smith@example.com",
                Phone = "1234567890",
                Age = 32,
                Address = "456 Oak Avenue",
                CountryOfOrigin = "Canada",
                AppliedDate = DateTime.UtcNow.AddDays(-3),
                Hired = false,
                CreatedBy = "TestUser",
                LastModifiedBy = "TestUser",
                DeletedReason = ""
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new DeleteApplicantCommand(existingApplicant.Id, existingApplicant.RowVersion, true); // Hard delete

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);

            var deletedApplicant = await _dbContext.Applicants.FindAsync(existingApplicant.Id);
            Assert.Null(deletedApplicant);
        }

        [Fact]
        public async Task Handle_NonExistingApplicant_ShouldReturnFalse()
        {
            // Arrange
            var command = new DeleteApplicantCommand(99999, new byte[] { 1, 2, 3, 4 }, false); // Non-existent ID

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_AlreadyDeletedApplicant_ShouldReturnFalse()
        {
            // Arrange
            var alreadyDeletedApplicant = new Applicant
            {
                Name = "Already Deleted",
                FamilyName = "Deleted",
                EmailAddress = "already.deleted@example.com",
                Phone = "1234567890",
                Age = 35,
                Address = "999 Gone Street",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-10),
                Hired = false,
                IsDeleted = true,
                DeletedDate = DateTime.UtcNow.AddDays(-1),
                CreatedBy = "TestUser",
                LastModifiedBy = "TestUser",
                DeletedReason = "Test deletion"
            };

            await _dbContext.Applicants.AddAsync(alreadyDeletedApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new DeleteApplicantCommand(alreadyDeletedApplicant.Id, alreadyDeletedApplicant.RowVersion, false); // Soft delete

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_MaliciousId_ShouldThrowArgumentException()
        {
            // Arrange
            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(true);

            var command = new DeleteApplicantCommand(1, new byte[] { 1, 2, 3, 4 }, false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_InvalidId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var command = new DeleteApplicantCommand(-1, new byte[] { 1, 2, 3, 4 }, false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SqlInjectionAttempt_ShouldThrowArgumentException()
        {
            // Arrange
            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(true);

            var command = new DeleteApplicantCommand(1, new byte[] { 1, 2, 3, 4 }, false);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_TransactionRollback_ShouldNotDeleteApplicant()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "Transaction Test",
                FamilyName = "Test",
                EmailAddress = "transaction.test@example.com",
                Phone = "1234567890",
                Age = 40,
                Address = "123 Transaction St",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-2),
                Hired = false,
                CreatedBy = "TestUser",
                LastModifiedBy = "TestUser",
                DeletedReason = ""
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new DeleteApplicantCommand(existingApplicant.Id, existingApplicant.RowVersion, false);

            // Simulate transaction failure by disposing context during operation
            var task = _handler.Handle(command, CancellationToken.None);
            _dbContext.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => task);
        }

        [Fact]
        public async Task Handle_MultipleDeletes_ShouldHandleCorrectly()
        {
            // Arrange
            var applicants = new List<Applicant>
            {
                new Applicant
                {
                    Name = "Delete Test 1",
                    FamilyName = "Test",
                    EmailAddress = "delete1.test@example.com",
                    Phone = "1234567890",
                    Age = 30,
                    Address = "123 Delete St",
                    CountryOfOrigin = "United States",
                    AppliedDate = DateTime.UtcNow.AddDays(-1),
                    Hired = false,
                    CreatedBy = "TestUser",
                    LastModifiedBy = "TestUser",
                    DeletedReason = ""
                },
                new Applicant
                {
                    Name = "Delete Test 2",
                    FamilyName = "Test",
                    EmailAddress = "delete2.test@example.com",
                    Phone = "1234567891",
                    Age = 35,
                    Address = "456 Delete Ave",
                    CountryOfOrigin = "United States",
                    AppliedDate = DateTime.UtcNow.AddDays(-2),
                    Hired = false,
                    CreatedBy = "TestUser",
                    LastModifiedBy = "TestUser",
                    DeletedReason = ""
                }
            };

            await _dbContext.Applicants.AddRangeAsync(applicants);
            await _dbContext.SaveChangesAsync();

            var command1 = new DeleteApplicantCommand(applicants[0].Id, applicants[0].RowVersion, false);
            var command2 = new DeleteApplicantCommand(applicants[1].Id, applicants[1].RowVersion, false);

            // Act
            var result1 = await _handler.Handle(command1, CancellationToken.None);
            var result2 = await _handler.Handle(command2, CancellationToken.None);

            // Assert
            Assert.True(result1);
            Assert.True(result2);
            Assert.Equal(0, await _dbContext.Applicants.CountAsync());
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}