using ApplicantManagement.Application.Features.Applicants.Queries.GetApplicantsWithFilters;
using ApplicantManagement.Applicants.Services;
using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Infrastructure.Data;
using ApplicantManagement.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicantManagement.Tests.Integration
{
    public class GetApplicantsWithFiltersHandlerIntegrationTests : IDisposable
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IApplicantRepository _applicantRepository;
        private readonly Mock<ILogger<GetApplicantsWithFiltersHandler>> _loggerMock;
        private readonly Mock<IApplicantLoggingService> _loggingServiceMock;
        private readonly Mock<IApplicantSecurityService> _securityServiceMock;
        private readonly GetApplicantsWithFiltersHandler _handler;

        public GetApplicantsWithFiltersHandlerIntegrationTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _dbContext = new ApplicationDbContext(options);
            
            // Setup repositories
            _applicantRepository = new ApplicantRepository(_dbContext, new Mock<ILogger<ApplicantRepository>>().Object);

            // Setup mocks
            _loggerMock = new Mock<ILogger<GetApplicantsWithFiltersHandler>>();
            _loggingServiceMock = new Mock<IApplicantLoggingService>();
            _securityServiceMock = new Mock<IApplicantSecurityService>();

            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(false);

            // Create handler
            _handler = new GetApplicantsWithFiltersHandler(
                _applicantRepository,
                _loggerMock.Object,
                _loggingServiceMock.Object,
                _securityServiceMock.Object);
        }

        [Fact]
        public async Task Handle_NoFilters_ShouldReturnAllActiveApplicants()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(3, result.Count); // Only active, non-deleted applicants
        }

        [Fact]
        public async Task Handle_AgeFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery
            {
                MinAge = 25,
                MaxAge = 35
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, applicant => Assert.InRange(applicant.Age, 25, 35));
        }

        [Fact]
        public async Task Handle_EmailSearch_ShouldReturnMatchingApplicants()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery
            {
                SearchTerm = "john"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, applicant => Assert.Contains("john", applicant.EmailAddress.ToLower()));
        }

        [Fact]
        public async Task Handle_CountryFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery
            {
                CountryOfOrigin = "United States"
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, applicant => Assert.Equal("United States", applicant.CountryOfOrigin));
        }

        [Fact]
        public async Task Handle_DateRangeFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery
            {
                AppliedDateFrom = DateTime.UtcNow.AddDays(-10),
                AppliedDateTo = DateTime.UtcNow.AddDays(-5)
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, applicant => 
            {
                Assert.True(applicant.AppliedDate >= query.AppliedDateFrom);
                Assert.True(applicant.AppliedDate <= query.AppliedDateTo);
            });
        }

        [Fact]
        public async Task Handle_HiredStatusFilter_ShouldReturnFilteredResults()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery
            {
                IsHired = true
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, applicant => Assert.True(applicant.Hired));
        }

        [Fact]
        public async Task Handle_SortingByName_ShouldReturnSortedResults()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery
            {
                SortBy = "name",
                SortDescending = false
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            var names = result.Select(a => a.Name).ToList();
            Assert.Equal(names.OrderBy(n => n), names);
        }

        [Fact]
        public async Task Handle_Pagination_ShouldReturnPaginatedResults()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery
            {
                PageNumber = 1,
                PageSize = 2
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task Handle_MaliciousInput_ShouldThrowArgumentException()
        {
            // Arrange
            _securityServiceMock.Setup(x => x.IsSqlInjectionAttempt(It.IsAny<string>()))
                .Returns(true);

            var query = new GetApplicantsWithFiltersQuery
            {
                SearchTerm = "<script>alert('xss')</script>"
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _handler.Handle(query, CancellationToken.None));
        }

        [Fact]
        public async Task Handle_ComplexFilters_ShouldReturnFilteredResults()
        {
            // Arrange
            await SeedTestDataAsync();
            var query = new GetApplicantsWithFiltersQuery
            {
                MinAge = 25,
                MaxAge = 35,
                CountryOfOrigin = "United States",
                IsHired = false,
                SortBy = "age",
                SortDescending = true,
                PageNumber = 1,
                PageSize = 10
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, applicant =>
            {
                Assert.InRange(applicant.Age, 25, 35);
                Assert.Equal("United States", applicant.CountryOfOrigin);
                Assert.False(applicant.Hired);
            });

            var ages = result.Select(a => a.Age).ToList();
            Assert.Equal(ages.OrderByDescending(a => a), ages);
        }

        private async Task SeedTestDataAsync()
        {
            var countries = new List<Country>
            {
                new Country { Id = 1, Name = "United States", Code = "US" },
                new Country { Id = 2, Name = "Canada", Code = "CA" },
                new Country { Id = 3, Name = "United Kingdom", Code = "UK" }
            };

            var applicants = new List<Applicant>
            {
                new Applicant
                {
                    Name = "John Doe",
                    FamilyName = "Doe",
                    EmailAddress = "john.doe@example.com",
                    Age = 28,
                    Address = "123 Main St",
                    CountryOfOrigin = "United States",
                    AppliedDate = DateTime.UtcNow.AddDays(-7),
                    Hired = false
                },
                new Applicant
                {
                    Name = "Jane Smith",
                    FamilyName = "Smith",
                    EmailAddress = "jane.smith@example.com",
                    Age = 32,
                    Address = "456 Oak Ave",
                    CountryOfOrigin = "Canada",
                    AppliedDate = DateTime.UtcNow.AddDays(-3),
                    Hired = false
                },
                new Applicant
                {
                    Name = "Bob Johnson",
                    FamilyName = "Johnson",
                    EmailAddress = "bob.johnson@example.com",
                    Age = 45,
                    Address = "789 Pine Rd",
                    CountryOfOrigin = "United States",
                    AppliedDate = DateTime.UtcNow.AddDays(-1),
                    Hired = false
                },
                new Applicant
                {
                    Name = "Alice Brown",
                    FamilyName = "Brown",
                    EmailAddress = "alice.brown@example.com",
                    Age = 22,
                    Address = "321 Elm St",
                    CountryOfOrigin = "United Kingdom",
                    AppliedDate = DateTime.UtcNow.AddDays(-14),
                    Hired = false
                },
                new Applicant
                {
                    Name = "Deleted User",
                    FamilyName = "Deleted",
                    EmailAddress = "deleted@example.com",
                    Age = 35,
                    Address = "999 Gone St",
                    CountryOfOrigin = "United States",
                    AppliedDate = DateTime.UtcNow.AddDays(-30),
                    Hired = false,
                    IsDeleted = true,
                    DeletedDate = DateTime.UtcNow.AddDays(-5)
                }
            };

            await _dbContext.Countries.AddRangeAsync(countries);
            await _dbContext.Applicants.AddRangeAsync(applicants);
            await _dbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _dbContext?.Dispose();
        }
    }
}