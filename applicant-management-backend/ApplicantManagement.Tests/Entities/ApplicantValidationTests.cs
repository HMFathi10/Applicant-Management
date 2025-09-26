using ApplicantManagement.Application.Features.Applicants.Commands.CreateApplicant;
using ApplicantManagement.Infrastructure.Services;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace ApplicantManagement.Tests.Entities
{
    public class ApplicantValidationTests
    {
        private readonly CreateApplicantValidator _validator;
        private readonly Mock<ICountryValidationService> _countryValidationServiceMock;

        public ApplicantValidationTests()
        {
            _countryValidationServiceMock = new Mock<ICountryValidationService>();
            _countryValidationServiceMock.Setup(x => x.ValidateCountryAsync(It.IsAny<string>()))
                .ReturnsAsync(true);

            _validator = new CreateApplicantValidator(_countryValidationServiceMock.Object);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Empty()
        {
            // Arrange
            var command = new CreateApplicantCommand { Name = "" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Have_Error_When_Name_Is_Too_Short()
        {
            // Arrange
            var command = new CreateApplicantCommand { Name = "John" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Name_Is_Valid()
        {
            // Arrange
            var command = new CreateApplicantCommand { Name = "John Smith" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Name);
        }

        [Fact]
        public void Should_Have_Error_When_Email_Is_Invalid()
        {
            // Arrange
            var command = new CreateApplicantCommand { EmailAddress = "invalid-email" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EmailAddress);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Email_Is_Valid()
        {
            // Arrange
            var command = new CreateApplicantCommand { EmailAddress = "test@example.com" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.EmailAddress);
        }

        [Fact]
        public void Should_Have_Error_When_Age_Is_Below_Minimum()
        {
            // Arrange
            var command = new CreateApplicantCommand { Age = 18 };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Age);
        }

        [Fact]
        public void Should_Have_Error_When_Age_Is_Above_Maximum()
        {
            // Arrange
            var command = new CreateApplicantCommand { Age = 65 };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Age);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Age_Is_Valid()
        {
            // Arrange
            var command = new CreateApplicantCommand { Age = 30 };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Age);
        }

        [Fact]
        public async Task Should_Have_Error_When_Country_Is_Invalid()
        {
            // Arrange
            _countryValidationServiceMock.Setup(x => x.ValidateCountryAsync("InvalidCountry"))
                .ReturnsAsync(false);
            var command = new CreateApplicantCommand { CountryOfOrigin = "InvalidCountry" };

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.CountryOfOrigin);
        }

        [Fact]
        public async Task Should_Not_Have_Error_When_Country_Is_Valid()
        {
            // Arrange
            _countryValidationServiceMock.Setup(x => x.ValidateCountryAsync("United States"))
                .ReturnsAsync(true);
            var command = new CreateApplicantCommand { CountryOfOrigin = "United States" };

            // Act
            var result = await _validator.TestValidateAsync(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.CountryOfOrigin);
        }

        [Fact]
        public void Should_Have_Error_When_Address_Is_Empty()
        {
            // Arrange
            var command = new CreateApplicantCommand { Address = "" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Address);
        }

        [Fact]
        public void Should_Have_Error_When_Address_Is_Too_Short()
        {
            // Arrange
            var command = new CreateApplicantCommand { Address = "123 Main" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Address);
        }

        [Fact]
        public void Should_Not_Have_Error_When_Address_Is_Valid()
        {
            // Arrange
            var command = new CreateApplicantCommand { Address = "123 Main Street, New York, NY 10001" };

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Address);
        }
    }
}