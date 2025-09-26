using ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantById;
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
    public class GetApplicantByIdHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IApplicantRepository _applicantRepository;
        private readonly Mock<ILogger<GetApplicantByIdHandler>> _loggerMock;
        private readonly Mock<IApplicantLoggingService> _loggingServiceMock;
        private readonly Mock<IApplicantSecurityService> _securityServiceMock;
        private readonly GetApplicantByIdHandler _handler;

        public GetApplicantByIdHandlerIntegrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            
            // Setup repositories
            _applicantRepository = new ApplicantRepository(_dbContext, new Mock<ILogger<ApplicantRepository>>().Object);

            // Setup mocks
            _loggerMock = new Mock<ILogger<GetApplicantByIdHandler>>();
            _loggingServiceMock = new Mock<IApplicantLoggingService>();
            _securityServiceMock = new Mock<IApplicantSecurityService>();

            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(false);

            // Create handler
            _handler = new GetApplicantByIdHandler(
                _applicantRepository,
                _loggerMock.Object,
                _loggingServiceMock.Object,
                _securityServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ExistingApplicant_ShouldReturnApplicant()
        {
            // Arrange
            var testApplicant = new Applicant
            {
                Name = "John Doe",
                FamilyName = "Doe",
                EmailAddress = "john.doe@example.com",
                Age = 28,
                Address = "123 Test Street",
                CountryOfOrigin = "United States",
                AppliedDate = DateTime.UtcNow.AddDays(-5),
                Hired = false
            };

            await _dbContext.Applicants.AddAsync(testApplicant);
            await _dbContext.SaveChangesAsync();

            var query = new GetApplicantByIdQuery(testApplicant.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testApplicant.Id, result.Id);
            Assert.Equal("John Doe", result.Name);
            Assert.Equal("john.doe@example.com", result.EmailAddress);
            Assert.Equal(28, result.Age);
        }

        [Fact]
        public async Task Handle_NonExistingApplicant_ShouldReturnNull()
        {
            // Arrange
            var query = new GetApplicantByIdQuery(99999); // Non-existent ID

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_DeletedApplicant_ShouldReturnNull()
        {
            // Arrange
            var deletedApplicant = new Applicant
            {
                Name = "Deleted User",
                FamilyName = "User",
                EmailAddress = "deleted@example.com",
                Age = 30,
                Address = "456 Deleted Street",
                CountryOfOrigin = "Canada",
                AppliedDate = DateTime.UtcNow.AddDays(-10),
                Hired = false,
                IsDeleted = true,
                DeletedDate = DateTime.UtcNow.AddDays(-1)
            };

            await _dbContext.Applicants.AddAsync(deletedApplicant);
            await _dbContext.SaveChangesAsync();

            var query = new GetApplicantByIdQuery(deletedApplicant.Id);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_MaliciousId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(true);

            var query = new GetApplicantByIdQuery(1);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_InvalidId_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var query = new GetApplicantByIdQuery(-1);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_SqlInjectionAttempt_ShouldThrowInvalidOperationException()
        {
            // Arrange
            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(true);

            var query = new GetApplicantByIdQuery(1);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, CancellationToken.None));
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}