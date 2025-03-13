using Entities;

namespace DTO
{
    public class CountryAddRequest
    {
        public string? CountryName { get; set; }


        public Country ToCountry() // this is a method for converting country add request to Country model for saving to db
        {
            return new Country
            {
                CountryName = this.CountryName ?? string.Empty // if country name null set it to empty
            };
        }
    }
}
