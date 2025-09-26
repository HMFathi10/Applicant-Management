using ApplicantManagement.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ApplicantManagement.Tests.Services
{
    public class CountryValidationServiceTests
    {
        private readonly Mock<ILogger<CountryValidationService>> _loggerMock;
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly CountryValidationService _service;

        public CountryValidationServiceTests()
        {
            _loggerMock = new Mock<ILogger<CountryValidationService>>();
            _handlerMock = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://restcountries.com/")
            };
            _service = new CountryValidationService(_httpClient, _loggerMock.Object);
        }

        [Fact]
        public async Task ValidateCountryAsync_ValidCountry_ReturnsTrue()
        {
            // Arrange
            var countryName = "United States";
            var countryData = new[]
            {
                new
                {
                    name = new { common = "United States", official = "United States of America" },
                    cca2 = "US",
                    region = "Americas"
                }
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(countryData))
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateCountryAsync(countryName);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public async Task ValidateCountryAsync_InvalidCountry_ReturnsFalse()
        {
            // Arrange
            var countryName = "NonExistentCountry";
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent("Not Found")
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _service.ValidateCountryAsync(countryName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetCountriesAsync_ReturnsCountryData()
        {
            // Arrange
            var countryData = new[]
            {
                new
                {
                    name = new { common = "United States", official = "United States of America" },
                    cca2 = "US",
                    region = "Americas"
                },
                new
                {
                    name = new { common = "Canada", official = "Canada" },
                    cca2 = "CA",
                    region = "Americas"
                }
            };

            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(countryData))
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetCountriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Contains(result, c => c.Name == "United States" && c.Code == "US" && c.Region == "Americas");
            Assert.Contains(result, c => c.Name == "Canada" && c.Code == "CA" && c.Region == "Americas");
        }

        [Fact]
        public async Task GetCountriesAsync_ApiError_ReturnsEmptyArray()
        {
            // Arrange
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Content = new StringContent("Server Error")
            };

            _handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            // Act
            var result = await _service.GetCountriesAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}