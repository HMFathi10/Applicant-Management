export interface Country {
  name: string;
  alpha2Code: string;
  alpha3Code: string;
  region: string;
}

export class CountryService {
  private countries: Country[] | null = null;

  async getCountries(): Promise<Country[]> {
    if (this.countries) {
      return this.countries;
    }

    try {
      // Import the local JSON file
      const countriesData = await import('../../Countries.json');
      const countries = countriesData.default || countriesData;
      
      if (!Array.isArray(countries)) {
        throw new Error('Invalid countries data format');
      }

      // Sort countries alphabetically by name
      this.countries = countries.sort((a: Country, b: Country) =>
        a.name.localeCompare(b.name)
      );
      
      return this.countries;
    } catch (error) {
      console.error('Error loading countries from local JSON:', error);
      return [];
    }
  }

  // Helper method to get country by name
  getCountryByName(name: string): Country | undefined {
    if (!this.countries) {
      return undefined;
    }
    return this.countries.find(country => 
      country.name.toLowerCase() === name.toLowerCase()
    );
  }

  // Helper method to get countries by region
  getCountriesByRegion(region: string): Country[] {
    if (!this.countries) {
      return [];
    }
    return this.countries.filter(country => 
      country.region.toLowerCase() === region.toLowerCase()
    );
  }
}

export const countryService = new CountryService();