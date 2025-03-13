using Entities;


namespace DTO
{
    public class CountryResponse
    {
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }


        public override bool Equals(object? obj)
        {
            if(obj==null)
            {
                return false;
            }
            if(obj.GetType()!=typeof(CountryResponse))
            {
                return false;
            }
            if(obj is CountryResponse countryResponse)
            {
                return countryResponse.CountryId == CountryId && countryResponse.CountryName == CountryName;  // ON THE TEST METHOD WE HAVE A MEthod called
                // assert.contains which calls equals , we need to compare each obj in country response and matching 
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    

    public static class CountryExtensions // Extension method which gets injected to country model
    {
        public static CountryResponse ToCountryResponse(this Country country) // this will determine this needs to inject to countRY model
        {
            CountryResponse countryResponse = new CountryResponse(); // converting country model to counry response. This way model gets hiiden from the user
            countryResponse.CountryId = country.CountryId;
            countryResponse.CountryName = country.CountryName;
            return countryResponse;
        }
    }


}
