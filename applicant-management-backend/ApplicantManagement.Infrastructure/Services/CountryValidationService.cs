using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ApplicantManagement.Infrastructure.Services
{
    public class CountryValidationService : ICountryValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CountryValidationService> _logger;
        private const string BaseUrl = "https://restcountries.com/v3.1/name/";

        public CountryValidationService(HttpClient httpClient, ILogger<CountryValidationService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> ValidateCountryAsync(string countryName)
        {
            if (string.IsNullOrWhiteSpace(countryName))
            {
                return false;
            }

            try
            {
                var url = $"{BaseUrl}{Uri.EscapeDataString(countryName)}?fullText=true";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var countries = JsonSerializer.Deserialize<JsonElement[]>(content);
                    return countries != null && countries.Length > 0;
                }

                _logger.LogWarning("Country validation failed for {CountryName}. Status code: {StatusCode}", 
                    countryName, response.StatusCode);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating country {CountryName}", countryName);
                return false;
            }
        }

        public bool ValidateCountry(string countryName)
        {
            return ValidateCountryAsync(countryName).GetAwaiter().GetResult();
        }

        public async Task<CountryData[]> GetCountriesAsync(string? searchTerm = null)
        {
            try
            {
                string url = string.IsNullOrWhiteSpace(searchTerm) 
                    ? "https://restcountries.com/v3.1/all?fields=name,cca2,region" 
                    : $"{BaseUrl}{Uri.EscapeDataString(searchTerm)}";

                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var countries = JsonSerializer.Deserialize<JsonElement[]>(content);
                    
                    if (countries == null || countries.Length == 0)
                    {
                        return Array.Empty<CountryData>();
                    }

                    var result = new CountryData[countries.Length];
                    
                    for (int i = 0; i < countries.Length; i++)
                    {
                        var country = countries[i];
                        var name = country.GetProperty("name").GetProperty("common").GetString();
                        var code = country.GetProperty("cca2").GetString();
                        var region = country.TryGetProperty("region", out var regionElement) 
                            ? regionElement.GetString() 
                            : "Unknown";

                        result[i] = new CountryData
                        {
                            Name = name,
                            Code = code,
                            Region = region
                        };
                    }

                    return result;
                }

                _logger.LogWarning("Failed to retrieve countries. Status code: {StatusCode}", response.StatusCode);
                return Array.Empty<CountryData>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving countries");
                return Array.Empty<CountryData>();
            }
        }
    }

    public class CountryData
    {
        public string Name { get; set; }
        public string Code { get; set; }
        public string Region { get; set; }
    }
}