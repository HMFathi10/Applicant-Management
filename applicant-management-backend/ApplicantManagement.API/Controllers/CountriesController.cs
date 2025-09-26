using ApplicantManagement.Domain.Entities;
using ApplicantManagement.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApplicantManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CountriesController : ControllerBase
    {
        private readonly IRepository<Country> _countryRepository;
        private readonly IMemoryCache _memoryCache;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CountriesController> _logger;
        private const string CountryCacheKey = "CountryList";

        public CountriesController(
            IRepository<Country> countryRepository,
            IMemoryCache memoryCache,
            IHttpClientFactory httpClientFactory,
            ILogger<CountriesController> logger)
        {
            _countryRepository = countryRepository;
            _memoryCache = memoryCache;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Country>>> GetAll()
        {
            try
            {
                _logger.LogInformation("Getting all countries");
                
                // Try to get countries from cache
                if (_memoryCache.TryGetValue(CountryCacheKey, out List<Country> countries))
                {
                    _logger.LogInformation("Retrieved {Count} countries from cache", countries.Count);
                    return Ok(countries);
                }

                // If not in cache, try to get from database
                _logger.LogInformation("Countries not found in cache, retrieving from database");
                var dbCountries = await _countryRepository.GetAllAsync(null);
                var countryList = new List<Country>(dbCountries);

                if (countryList.Count == 0)
                {
                    // If not in database, fetch from external API
                    _logger.LogInformation("No countries found in database, fetching from external API");
                    countryList = await FetchCountriesFromExternalApi();
                    
                    // Save to database
                    _logger.LogInformation("Saving {Count} countries to database", countryList.Count);
                    foreach (var country in countryList)
                    {
                        await _countryRepository.AddAsync(country);
                    }
                    await _countryRepository.SaveChangesAsync();
                }
                else
                {
                    _logger.LogInformation("Retrieved {Count} countries from database", countryList.Count);
                }

                // Cache the result
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24));
                
                _memoryCache.Set(CountryCacheKey, countryList, cacheOptions);
                _logger.LogInformation("Cached {Count} countries", countryList.Count);

                return Ok(countryList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving countries");
                return StatusCode(500, "An error occurred while processing your request");
            }
        }

        private async Task<List<Country>> FetchCountriesFromExternalApi()
        {
            try
            {
                _logger.LogInformation("Fetching countries from external API");
                var client = _httpClientFactory.CreateClient();
                var apiUrl = "https://api.countrylayer.com/v2/all?access_key=ea9afe93b847b35e3fb2cef26d06785d";
                
                _logger.LogInformation("Sending request to {ApiUrl}", apiUrl);
                var response = await client.GetAsync(apiUrl);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully received response from external API");
                    var content = await response.Content.ReadAsStringAsync();
                    var countries = JsonSerializer.Deserialize<List<CountryApiResponse>>(content);
                    
                    var result = new List<Country>();
                    int id = 1;
                    
                    foreach (var country in countries)
                    {
                        result.Add(new Country
                        {
                            Id = id++,
                            Name = country.Name,
                            Code = country.Alpha2Code
                        });
                    }
                    
                    _logger.LogInformation("Processed {Count} countries from external API", result.Count);
                    return result;
                }
                else
                {
                    _logger.LogWarning("Failed to fetch countries from external API. Status code: {StatusCode}", response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching countries from external API");
            }
            
            return new List<Country>();
        }

        private class CountryApiResponse
        {
            public string Name { get; set; }
            public string Alpha2Code { get; set; }
        }
    }
}