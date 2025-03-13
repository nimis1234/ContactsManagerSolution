using AutoFixture;
using DTO;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using FluentAssertions.Execution;
using Interfaces;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Repository;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

/*
 
 for testing repo , mocked db context 
 
 */
namespace CrudTest
{
    public class CountryRepoTest
    {
        private readonly CountryRepo _countryRepo;
        private readonly IFixture _fixture;

        public CountryRepoTest()
        {
            // instead of using Inmemory database , we can mock the database


            /*var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                        .UseInMemoryDatabase(databaseName: "TestDatabase")
                        .Options; 

            _dbContext = new ApplicationDbContext(options);
            countryServices = new CountryServices(_dbContext);*/

            _fixture = new Fixture();
            var countryService = new List<Country>();
            DbContextMock<ApplicationDbContext> dbContextMock = new DbContextMock<ApplicationDbContext>
                                         (new DbContextOptionsBuilder<ApplicationDbContext>().Options); // Initializes a mocked ApplicationDbContext without a real database.

            ApplicationDbContext dbContext = dbContextMock.Object; //Retrieves the mocked DbContext object for use in tests.

            dbContextMock.CreateDbSetMock(temp => temp.Countries, countryService);//Mocks the DbSet<Country> using the countryService list as data.

            // 2. Mock InsertCountryNameViaSP to add to in-memory Countries
            dbContextMock.Setup(x => x.InsertCountryNameViaSP(
                    It.IsAny<string>(),
                    It.IsAny<Guid>()
                ))
                .Returns((string name, Guid id) =>
                {
                    dbContext.Countries.Add(new Country
                    {
                        CountryId = id,
                        CountryName = name
                    });
                    return dbContext.SaveChanges();
                });
            _countryRepo = new CountryRepo(dbContext); // Passes the mocked DbContext into the service, replacing real database interactions.

        }

        #region AddCountry

        // we need to test following test for AddCountryService
        /// <summary>
        /// check the CountryAddRequest is not null
        /// check CountryName is not Null and not have space or special charcters in first
        /// Check Country Name is not duplicate
        /// insert proper data
        /// </summary>

        [Fact]
        // check the CountryAddRequest is not null
        public async Task AddCountry_NullCountryAddRequest()
        {
            // Arrange
            Country country = null;
            // assert and act together
            await Assert.ThrowsAsync<ArgumentNullException>( async () =>
            {
                await _countryRepo.AddCountry(country);
            }
            );

        }

        [Fact]
        // check CountryName is not Null and not have space or special charcters in first
        public async Task AddCountry_NullCountryName()
        {
            // Arrange
            Country country = _fixture.Build<Country>().Without(temp => temp.Persons).With(temp => temp.CountryName, null as string).Create();
            // assert and act together
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _countryRepo.AddCountry(country);
            }
            );


        }


        [Fact]
        // Check Country Name is not duplicate
        public async Task  AddCountry_DuplicateCountryName()
        {
            // Arrange
            Country country1 = _fixture.Build<Country>().With(temp => temp.CountryName, "India"). Without(temp => temp.Persons).Create(); // Optional: Avoid circular references.Create();
            Country country2 = _fixture.Build<Country>().With(temp => temp.CountryName, "India").Without(temp => temp.Persons).Create();

            //act

            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _countryRepo.AddCountry(country1);
                await _countryRepo.AddCountry(country2);

            });

        }



        [Fact]
        // insert proper data
        public async Task AddCountry_AddCountryProperData()
        {
            //arrage
            CountryAddRequest countryAddRequest = new CountryAddRequest();
            countryAddRequest.CountryName = "Malasia";

            Country countryExpected = countryAddRequest.ToCountry();
            //act
            var CountryAdded = await _countryRepo.AddCountry(countryExpected);

            //fluent assertion
            CountryAdded.Should().BeEquivalentTo(countryExpected);
            

        }

        #endregion  AddCountry


        #region  Get All Country Test



        [Fact]
        // null case
        public async Task GetAllCountry_GetAllCountry()
        {
            // Arrange

            List<CountryAddRequest> countryAddRequestList = _fixture.Create<List<CountryAddRequest>>();
            var CountryExepectedToAdd = countryAddRequestList.Select(temp => temp.ToCountry()).ToList();
            // act
            //loop it and call AddCountry

            foreach (var country in CountryExepectedToAdd)
            {
                var countryResponse = await  _countryRepo.AddCountry(country);
            }

            //now get all country
            var GetAllCountryResult = await _countryRepo.GetAllCountries();

            //fluent assertion
            GetAllCountryResult.Should().BeEquivalentTo(CountryExepectedToAdd);
        }

        #endregion

        #region getCountryById
        [Fact]
        public void GetCountryById_GetCountryById()
        {
            // Arrange

            List<CountryAddRequest> countryAddRequestList = new List<CountryAddRequest>()
            {
                new CountryAddRequest() { CountryName = "India" },
                new CountryAddRequest() { CountryName = "America" },
                new CountryAddRequest() { CountryName = "Australia" }

            };

            var CountryExepectedToAdd = countryAddRequestList.Select(temp => temp.ToCountry()).ToList();
            // act
            //loop it and call AddCountry

            foreach (var country in CountryExepectedToAdd)
            {
                var countryResponse = _countryRepo.AddCountry(country);
            }

           
            // now get country by CountryId

            var GetCountryByIdResult = _countryRepo.GetCountryById(CountryExepectedToAdd[1].CountryId).Result;

            //  find  object present or not
            //Assert.Contains(GetCountryByIdResult, CountryExepectedToAdd); // check the object is present or not

            //fluent assertion
            GetCountryByIdResult.Should().BeEquivalentTo(CountryExepectedToAdd[1]);
        }
        #endregion


        #region NullGetCountryById
        [Fact]
        public async Task GetCountryById_NullGetCountryById()
        {
            var GetCountryByIdResult = await _countryRepo.GetCountryById(null);

            //  find  object present or not
            Assert.Null(GetCountryByIdResult);
        }
        #endregion


    }
}