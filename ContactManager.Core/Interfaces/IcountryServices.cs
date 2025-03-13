using DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Interfaces
{
    public  interface IcountryServices
    {
        Task<CountryResponse> AddCountry(CountryAddRequest countryAddRequest);
        Task<List<string>> ExtractCountryExcelFile (IFormFile file);
        Task<List<CountryResponse>> GetAllCountries();
         Task<CountryResponse?> GetCountryById(Guid? id);
        
    }
}
