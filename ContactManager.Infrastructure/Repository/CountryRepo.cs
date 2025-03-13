using DTO;
using Entities;
using IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repository
{
    public  class CountryRepo : ICountryRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public CountryRepo (ApplicationDbContext dbContext)
        {
           _dbContext= dbContext;
        }


        public async Task<Country> AddCountry(Country country)
        {
            if(country == null)
            {
                throw new ArgumentNullException(nameof(country), "Country cannot be null");
            }
            if (country.CountryName == null)
            {
                throw new ArgumentException(nameof(country.CountryName), "Country name cannot be null");
            }

            // error out if duplicates are found
            if (_dbContext.Countries.Any(x => x.CountryName == country.CountryName))
            {
                throw new ArgumentException($"country with name {country.CountryName} already exists");
            }

            country.CountryId = Guid.NewGuid();
            // instead call stored procedure
            await Task.Run(() => _dbContext.InsertCountryNameViaSP(country.CountryName, country.CountryId));

            //_dbContext.Countries.Add(convertToCountry); // add country to list
            //_dbContext.SaveChanges(); // save changes to db
            return country;
        }


        public async Task<List<Country>> GetAllCountries()
        {
            return await _dbContext.Countries.ToListAsync();
        }

        public async Task<Country?> GetCountryById(Guid? id)
        {
           return await _dbContext.Countries.FirstOrDefaultAsync(x => x.CountryId == id);
        }
    }
}
