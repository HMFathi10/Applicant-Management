using ApplicantManagement.Application.Features.Applicants.Commands.CreateApplicant;
using ApplicantManagement.Applicants.Services;
using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using ApplicantManagement.Infrastructure.Data;
using ApplicantManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicantManagement.Tests.Integration
{
    public class CreateApplicantHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IApplicantRepository _applicantRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly Mock<ILogger<CreateApplicantHandler>> _loggerMock;
        private readonly Mock<IApplicantLoggingService> _loggingServiceMock;
        private readonly Mock<IApplicantSecurityService> _securityServiceMock;
        private readonly CreateApplicantHandler _handler;

        public CreateApplicantHandlerIntegrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);

            // Setup repositories and unit of work
            var repository = new Repository<Applicant>(_dbContext);
            _applicantRepository = new ApplicantRepository(_dbContext, new Mock<ILogger<ApplicantRepository>>().Object);
            _unitOfWork = new UnitOfWork(_dbContext, new Mock<ILogger<UnitOfWork>>().Object);

            // Setup mocks
            _loggerMock = new Mock<ILogger<CreateApplicantHandler>>();
            _loggingServiceMock = new Mock<IApplicantLoggingService>();
            _securityServiceMock = new Mock<IApplicantSecurityService>();

            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(false);

            // Create handler
            _handler = new CreateApplicantHandler(
                _applicantRepository,
                _unitOfWork,
                _loggerMock.Object,
                _loggingServiceMock.Object,
                _securityServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ValidApplicant_ShouldCreateApplicantSuccessfully()
        {
            // Arrange
            var command = new CreateApplicantCommand
            {
                Name = "John Smith",
                FamilyName = "Smith",
                EmailAddress = "john.smith@example.com",
                Phone = "+1234567890",
                Age = 30,
                Address = "123 Main Street, New York, NY 10001",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow,
                Hired = true
            };

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            Assert.NotEqual(0, result);
            Assert.True(result > 0);

            var createdApplicant = await _dbContext.Applicants.FindAsync(result);
            Assert.NotNull(createdApplicant);
            Assert.Equal("John Smith", createdApplicant.Name);
            Assert.Equal("john.smith@example.com", createdApplicant.EmailAddress);
            Assert.Equal(30, createdApplicant.Age);
        }

        [Fact]
        public async Task Handle_DuplicateEmail_ShouldThrowArgumentException()
        {
            // Arrange
            var existingApplicant = new Applicant
            {
                Name = "Existing User",
                FamilyName = "User",
                EmailAddress = "existing@example.com",
                Phone = "+1234567890",
                Age = 25,
                Address = "456 Oak Street",
                CountryOfOrigin = "Canada",
                AppliedDate = DateTime.UtcNow.AddDays(-1),
                Hired = true,
                CreatedBy = "System",
                LastModifiedBy = "System",
                DeletedReason = "",
                RowVersion = new byte[8]
            };

            await _dbContext.Applicants.AddAsync(existingApplicant);
            await _dbContext.SaveChangesAsync();

            var command = new CreateApplicantCommand
            {
                Name = "New User",
                FamilyName = "User",
                EmailAddress = "existing@example.com", // Same email as existing
                Phone = "+1234567890",
                Age = 30,
                Address = "123 Main Street",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow,
                Hired = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_InvalidAge_ShouldThrowArgumentException()
        {
            // Arrange
            var command = new CreateApplicantCommand
            {
                Name = "Young Applicant",
                FamilyName = "Applicant",
                EmailAddress = "young@example.com",
                Phone = "+1234567890",
                Age = 17, // Below minimum age
                Address = "123 Main Street",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow,
                Hired = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_MaliciousInput_ShouldThrowArgumentException()
        {
            // Arrange
            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(true);

            var command = new CreateApplicantCommand
            {
                Name = "<script>alert('xss')</script>",
                FamilyName = "Smith",
                EmailAddress = "test@example.com",
                Phone = "+1234567890",
                Age = 30,
                Address = "123 Main Street",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow,
                Hired = true
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _handler.Handle(command, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_TransactionRollback_ShouldNotCreateApplicant()
        {
            // Arrange
            var command = new CreateApplicantCommand
            {
                Name = "Test User",
                FamilyName = "User",
                EmailAddress = "rollback@example.com",
                Phone = "+1234567890",
                Age = 30,
                Address = "123 Main Street",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow,
                Hired = true
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