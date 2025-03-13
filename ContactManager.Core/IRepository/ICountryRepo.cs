using Entities;


namespace IRepository
{

    // repository is used connect to database and perform all db related acton
    // follow  controller-> services-> repository-> database
    public interface ICountryRepo
    {
        Task<Country> AddCountry(Country country);
        
        Task<List<Country>> GetAllCountries();
        Task<Country?> GetCountryById(Guid? id);

    }
}
