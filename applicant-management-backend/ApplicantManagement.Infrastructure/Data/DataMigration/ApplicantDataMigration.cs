using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ApplicantManagement.Infrastructure.Data.DataMigration
{
    public class ApplicantDataMigration
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ICountryValidationService _countryValidationService;
        private readonly ILogger<ApplicantDataMigration> _logger;

        public ApplicantDataMigration(
            ApplicationDbContext dbContext,
            ICountryValidationService countryValidationService,
            ILogger<ApplicantDataMigration> logger)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _countryValidationService = countryValidationService ?? throw new ArgumentNullException(nameof(countryValidationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task MigrateDataAsync()
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                await SynchronizeCountriesAsync();
                await InsertSampleApplicantsAsync();
                
                await transaction.CommitAsync();
                _logger.LogInformation("Data migration completed successfully");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error during data migration");
                throw;
            }
        }

        private async Task SynchronizeCountriesAsync()
        {
            var countries = await _countryValidationService.GetCountriesAsync();
            
            foreach (var countryData in countries)
            {
                var existingCountry = await _dbContext.Countries
                    .FirstOrDefaultAsync(c => c.Name.Equals(countryData.Name, StringComparison.OrdinalIgnoreCase));

                if (existingCountry == null)
                {
                    _dbContext.Countries.Add(new Country
                    {
                        Name = countryData.Name,
                        Region = countryData.Region,
                        IsActive = true
                    });
                }
                else
                {
                    existingCountry.Region = countryData.Region;
                    existingCountry.IsActive = true;
                }
            }

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Synchronized {Count} countries", countries.Length);
        }

        private async Task InsertSampleApplicantsAsync()
        {
            if (await _dbContext.Applicants.AnyAsync())
            {
                _logger.LogInformation("Sample applicants already exist, skipping insertion");
                return;
            }

            var countries = await _dbContext.Countries.ToListAsync();
            if (!countries.Any())
            {
                _logger.LogWarning("No countries found for sample applicants");
                return;
            }

            var sampleApplicants = new List<Applicant>
            {
                new Applicant
                {
                    Name = "John",
                    FamilyName = "Smith",
                    Address = "123 Main Street, New York, NY 10001",
                    EmailAddress = "john.smith@example.com",
                    Phone = "+201234567890",
                    Age = 35,
                    CountryOfOrigin = countries[0].Name,
                    AppliedDate = DateTime.Now.AddDays(-30),
                    Hired = false
                },
                new Applicant
                {
                    Name = "Sarah",
                    FamilyName = "Johnson",
                    Address = "456 Park Avenue, Boston, MA 02108",
                    EmailAddress = "sarah.johnson@example.com",
                    Phone = "+201234567891",
                    Age = 28,
                    CountryOfOrigin = countries[1 % countries.Count].Name,
                    AppliedDate = DateTime.Now.AddDays(-25),
                    Hired = false
                },
                new Applicant
                {
                    Name = "Michael",
                    FamilyName = "Williams",
                    Address = "789 Broadway, San Francisco, CA 94105",
                    EmailAddress = "michael.williams@example.com",
                    Phone = "+201234567892",
                    Age = 42,
                    CountryOfOrigin = countries[2 % countries.Count].Name,
                    AppliedDate = DateTime.Now.AddDays(-20),
                    Hired = true
                },
                new Applicant
                {
                    Name = "Emily",
                    FamilyName = "Brown",
                    Address = "321 Oak Street, Chicago, IL 60601",
                    EmailAddress = "emily.brown@example.com",
                    Phone = "+201234567893",
                    Age = 31,
                    CountryOfOrigin = countries[3 % countries.Count].Name,
                    AppliedDate = DateTime.Now.AddDays(-15),
                    Hired = false
                },
                new Applicant
                {
                    Name = "David",
                    FamilyName = "Miller",
                    Address = "654 Pine Street, Seattle, WA 98101",
                    EmailAddress = "david.miller@example.com",
                    Phone = "+201234567894",
                    Age = 45,
                    CountryOfOrigin = countries[4 % countries.Count].Name,
                    AppliedDate = DateTime.Now.AddDays(-10),
                    Hired = false
                }
            };

            await _dbContext.Applicants.AddRangeAsync(sampleApplicants);
            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Inserted {Count} sample applicants", sampleApplicants.Count);
        }
    }
}