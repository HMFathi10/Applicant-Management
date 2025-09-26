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
    public class UpdateApplicantHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IApplicantRepository _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<ILogger<UpdateApplicantHandler>> _loggerMock;
        private readonly Mock<IApplicantLoggingService> _loggingServiceMock;
        private readonly Mock<IApplicantSecurityService> _securityServiceMock;
        private readonly UpdateApplicantHandler _handler;

        public UpdateApplicantHandlerIntegrationTests()
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
            _loggerMock = new Mock<ILogger<UpdateApplicantHandler>>();
            _loggingServiceMock = new Mock<IApplicantLoggingService>();
            _securityServiceMock = new Mock<IApplicantSecurityService>();

            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(false);

            // Create handler
            _handler = new UpdateApplicantHandler(
                _applicantRepository,
                _unitOfWork,
                _loggerMock.Object,
                _loggingServiceMock.Object,
                _securityServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidUpdate_ShouldUpdateApplicantSuccessfully()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "Original Name",
                FamilyName = "Original Family",
                EmailAddress = "original@example.com",
                Age = 25,
                Address = "Original Address",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-10),
                Hired = false
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new UpdateApplicantCommand
            {
                Id = existingApplicant.Id,
                Name = "Updated Name",
                FamilyName = "Updated Family",
                EmailAddress = "updated@example.com",
                Age = 30,
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = false
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.True(result);

            var updatedApplicant = await _dbContext.Applicants.FindAsync(existingApplicant.Id);
            Assert.NotNull(updatedApplicant);
            Assert.Equal("Updated Name", updatedApplicant.Name);
            Assert.Equal("updated@example.com", updatedApplicant.EmailAddress);
            Assert.Equal(30, updatedApplicant.Age);
            Assert.Equal("Canada", updatedApplicant.CountryOfOrigin);
            Assert.False(updatedApplicant.Hired);
        }

        [Fact]
        public async Task Handle_NonExistingApplicant_ShouldReturnFalse()
        {
            // Arrange
            var command = new UpdateApplicantCommand
            {
                Id = 99999, // Non-existent ID
                Name = "Updated Name",
                FamilyName = "Updated Family",
                EmailAddress = "updated@example.com",
                Age = 30,
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = false
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var existingApplicant1 = new Applicant
            {
                Name = "User One",
                FamilyName = "One",
                EmailAddress = "user1@example.com",
                Age = 25,
                Address = "Address 1",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-10),
                Hired = false
            };

            var existingApplicant2 = new Applicant
            {
                Name = "User Two",
                FamilyName = "Two",
                EmailAddress = "user2@example.com",
                Age = 30,
                Address = "Address 2",
                CountryOfOrigin = "Canada",
                AppliedDate = DateTime.UtcNow.AddDays(-5),
                Hired = false
            };

            await _dbContext.Applicants.AddRangeAsync(existingApplicant1, existingApplicant2);
            await _dbContext.SaveChangesAsync();

            var command = new UpdateApplicantCommand
            {
                Id = existingApplicant2.Id,
                Name = "Updated User Two",
                FamilyName = "Updated Two",
                EmailAddress = "user1@example.com", // Same as existingApplicant1
                Age = 35,
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_InvalidAge_ShouldThrowArgumentException()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "Existing User",
                FamilyName = "User",
                EmailAddress = "existing@example.com",
                Age = 25,
                Address = "Existing Address",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-10),
                Hired = false
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new UpdateApplicantCommand
            {
                Id = existingApplicant.Id,
                Name = "Updated Name",
                FamilyName = "Updated Family",
                EmailAddress = "updated@example.com",
                Age = 17, // Invalid age
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_MaliciousInput_ShouldThrowArgumentException()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "Existing User",
                FamilyName = "User",
                EmailAddress = "existing@example.com",
                Age = 25,
                Address = "Existing Address",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-10),
                Hired = false
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(true);

            var command = new UpdateApplicantCommand
            {
                Id = existingApplicant.Id,
                Name = "<script>alert('xss')</script>",
                FamilyName = "Updated Family",
                EmailAddress = "updated@example.com",
                Age = 30,
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_DeletedApplicant_ShouldReturnFalse()
        {
            // Arrange
            var deletedApplicant = new Applicant
            {
                Name = "Deleted User",
                FamilyName = "Deleted",
                EmailAddress = "deleted@example.com",
                Age = 30,
                Address = "Deleted Address",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-10),
                Hired = false,
                IsDeleted = true,
                DeletedDate = DateTime.UtcNow.AddDays(-1)
            };

            await _dbContext.Applicants.AddAsync(deletedApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new UpdateApplicantCommand
            {
                Id = deletedApplicant.Id,
                Name = "Updated Deleted User",
                FamilyName = "Updated Family",
                EmailAddress = "updated.deleted@example.com",
                Age = 35,
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = false
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task Handle_TransactionRollback_ShouldNotUpdateApplicant()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "Original Name",
                FamilyName = "Original Family",
                EmailAddress = "original@example.com",
                Age = 25,
                Address = "Original Address",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-10),
                Hired = false
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new UpdateApplicantCommand
            {
                Id = existingApplicant.Id,
                Name = "Updated Name",
                FamilyName = "Updated Family",
                EmailAddress = "updated@example.com",
                Age = 30,
                Address = "Updated Address",
                CountryOfOrigin = "Canada",
                Hired = false
            };

            // Simulate transaction failure by disposing context during operation
            var task = _handler.Handle(command, CancellationToken.None);
            _dbContext.Dispose();

            // Act & Assert
            await Assert.ThrowsAsync<ObjectDisposedException>(() => task);
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}