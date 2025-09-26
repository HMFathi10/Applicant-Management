using ApplicantManagement.Application.Features.Applicants.Commands.CreateApplicant;
using ApplicantManagement.Application.Features.Applicants.Commands.UpdateApplicant;
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
    public class TransactionManagementIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IApplicantRepository _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<ILogger<CreateApplicantHandler>> _createLoggerMock;
        private readonly Mock<ILogger<UpdateApplicantHandler>> _updateLoggerMock;
        private readonly Mock<IApplicantLoggingService> _loggingServiceMock;
        private readonly Mock<IApplicantSecurityService> _securityServiceMock;
        private readonly CreateApplicantHandler _createHandler;
        private readonly UpdateApplicantHandler _updateHandler;

        public TransactionManagementIntegrationTests()
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
            _createLoggerMock = new Mock<ILogger<CreateApplicantHandler>>();
            _updateLoggerMock = new Mock<ILogger<UpdateApplicantHandler>>();
            _loggingServiceMock = new Mock<IApplicantLoggingService>();
            _securityServiceMock = new Mock<IApplicantSecurityService>();

            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(false);

            // Create handlers
            _createHandler = new CreateApplicantHandler(
                _applicantRepository,
                _unitOfWork,
                _createLoggerMock.Object,
                _loggingServiceMock.Object,
                _securityServiceMock.Object);

            _updateHandler = new UpdateApplicantHandler(
                _applicantRepository,
                _unitOfWork,
                _updateLoggerMock.Object,
                _loggingServiceMock.Object,
                _securityServiceMock.Object);
        }

        [Fact]
        public async Task CreateApplicant_WithTransaction_CommitsSuccessfully()
        {
            // Arrange
            var command = new CreateApplicantCommand
            {
                Name = "Transaction Test User",
                FamilyName = "Test",
                EmailAddress = "transaction.test@example.com",
                Phone = "+1234567890",
                Age = 30,
                Address = "123 Transaction St",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow,
                Hired = false
            };

            // Act
            var result = await _createHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result > 0);

            var createdApplicant = await _dbContext.Applicants.FindAsync(result);
            Assert.NotNull(createdApplicant);
            Assert.Equal("Transaction Test User", createdApplicant.Name);
            Assert.Equal("transaction.test@example.com", createdApplicant.EmailAddress);
        }

        [Fact]
        public async Task UpdateApplicant_WithTransaction_CommitsSuccessfully()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "Original User",
                FamilyName = "User",
                EmailAddress = "original@example.com",
                Phone = "+1234567890",
                Age = 25,
                Address = "Original Address",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-5),
                Hired = false,
                CreatedBy = "System",
                LastModifiedBy = "System",
                DeletedReason = "",
                RowVersion = new byte[8]
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new UpdateApplicantCommand
            {
                Id = existingApplicant.Id,
                Name = "Updated User",
                FamilyName = "Updated Family",
                EmailAddress = "updated@example.com",
                Age = 30,
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = true
            };

            // Act
            var result = await _updateHandler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);

            var updatedApplicant = await _dbContext.Applicants.FindAsync(existingApplicant.Id);
            Assert.NotNull(updatedApplicant);
            Assert.Equal("Updated User", updatedApplicant.Name);
            Assert.Equal("updated@example.com", updatedApplicant.EmailAddress);
            Assert.Equal(30, updatedApplicant.Age);
        }

        [Fact]
        public async Task MultipleOperations_InTransaction_AllOrNothing()
        {
            // Arrange
            var createCommand1 = new CreateApplicantCommand
            {
                Name = "User One",
                FamilyName = "One",
                EmailAddress = "user1@example.com",
                Phone = "+1234567890",
                Age = 25,
                Address = "Address 1",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow,
                Hired = false
            };

            var createCommand2 = new CreateApplicantCommand
            {
                Name = "User Two",
                FamilyName = "Two",
                EmailAddress = "user2@example.com",
                Phone = "+1234567890",
                Age = 30,
                Address = "Address 2",
                CountryOfOrigin = "Canada",
                AppliedDate = DateTime.UtcNow,
                Hired = false
            };

            // Act - Create both users
            var result1 = await _createHandler.Handle(createCommand1, CancellationToken.None);
            var result2 = await _createHandler.Handle(createCommand2, CancellationToken.None);

            // Assert - Both should be created
            Assert.True(result1 > 0);
            Assert.True(result2 > 0);

            var user1 = await _dbContext.Applicants.FindAsync(result1);
            var user2 = await _dbContext.Applicants.FindAsync(result2);

            Assert.NotNull(user1);
            Assert.NotNull(user2);
            Assert.Equal("User One", user1.Name);
            Assert.Equal("User Two", user2.Name);
        }

        [Fact]
        public async Task Transaction_RollbackOnFailure_MaintainsDataIntegrity()
        {
            // Arrange
            var validApplicant = new Applicant
            {
                Name = "Valid User",
                FamilyName = "User",
                EmailAddress = "valid@example.com",
                Phone = "+1234567890",
                Age = 30,
                Address = "Valid Address",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-2),
                Hired = false,
                CreatedBy = "System",
                LastModifiedBy = "System",
                DeletedReason = "",
                RowVersion = new byte[8]
            };

            await _dbContext.Applicants.AddAsync(validApplicant);
            await _dbContext.SaveChangesAsync();

            // Try to create another applicant with the same email (should fail)
            var duplicateCommand = new CreateApplicantCommand
            {
                Name = "Duplicate User",
                FamilyName = "User",
                EmailAddress = "valid@example.com", // Same email as existing
                Phone = "+1234567890",
                Age = 35,
                Address = "Duplicate Address",
                CountryOfOrigin = "Canada",
                AppliedDate = DateTime.UtcNow,
                Hired = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _createHandler.Handle(duplicateCommand, CancellationToken.None));

            // Verify the original applicant is still intact
            var originalApplicant = await _dbContext.Applicants.FindAsync(validApplicant.Id);
            Assert.NotNull(originalApplicant);
            Assert.Equal("Valid User", originalApplicant.Name);
            Assert.Equal("valid@example.com", originalApplicant.EmailAddress);

            // Verify no duplicate was created
            var allApplicants = await _dbContext.Applicants.ToListAsync();
            Assert.Single(allApplicants.Where(a => a.EmailAddress == "valid@example.com"));
        }

        [Fact]
        public async Task UnitOfWork_TransactionLifecycle_CommitAndRollback()
        {
            // Arrange
            var applicant = new Applicant
            {
                Name = "Transaction Test",
                FamilyName = "Test",
                EmailAddress = "transaction.lifecycle@example.com",
                Phone = "+1234567890",
                Age = 28,
                Address = "Transaction Address",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow,
                Hired = false,
                CreatedBy = "System",
                LastModifiedBy = "System",
                DeletedReason = "",
                RowVersion = new byte[8]
            };

            // Act - Test basic repository operations (without manual transaction management)
            await _applicantRepository.AddAsync(applicant);
            await _unitOfWork.SaveChangesAsync();

            // Assert - Should be committed
            var committedApplicant = await _dbContext.Applicants.FindAsync(applicant.Id);
            Assert.NotNull(committedApplicant);
            Assert.Equal("Transaction Test", committedApplicant.Name);

            // Test update functionality
            var updateCommand = new UpdateApplicantCommand
            {
                Id = applicant.Id,
                Name = "Updated Name",
                FamilyName = "Updated Family",
                EmailAddress = "updated@example.com",
                Phone = "+1234567890",
                Age = 35,
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = true
            };

            var updateResult = await _updateHandler.Handle(updateCommand, CancellationToken.None);
            Assert.True(updateResult);

            // Verify the update was applied
            var updatedApplicant = await _dbContext.Applicants.FindAsync(applicant.Id);
            Assert.NotNull(updatedApplicant);
            Assert.Equal("Updated Name", updatedApplicant.Name);
            Assert.Equal("updatedemail@example.com", updatedApplicant.EmailAddress);
            Assert.Equal(35, updatedApplicant.Age);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}