using System.Threading.Tasks;

namespace ApplicantManagement.Infrastructure.Services
{
    public interface ICountryValidationService
    {
        Task<bool> ValidateCountryAsync(string countryName);
        bool ValidateCountry(string countryName);
        Task<CountryData[]> GetCountriesAsync(string? searchTerm = null);
    }
}