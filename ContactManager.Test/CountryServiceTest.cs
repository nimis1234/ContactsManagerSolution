using AutoFixture;
using Castle.Core.Resource;
using DTO;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using Interfaces;
using IRepository;
using Microsoft.AspNetCore.Routing.Template;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using System.Runtime.CompilerServices;
using Xunit;

namespace CrudTest
{ 
    public class CountryServiceTest 
    {
        private readonly Fixture _fixture;
        private readonly Mock<ICountryRepo> _countryMockRepository;
        private readonly Mock<ILogger<CountryServices>> _mocklogger;
        private readonly ICountryRepo _countryRepository;
        private readonly CountryServices _countryServices;
        private readonly DbContextMock<ApplicationDbContext> dbContextMock;
        private readonly ApplicationDbContext dbContext;

        public CountryServiceTest()
        {
            _fixture = new Fixture();
            var countriesInitialData = new List<Country>();
            //mock country repo
            _countryMockRepository = new Mock<ICountryRepo>();

            _mocklogger=new Mock<ILogger<CountryServices>>();
            //create mock country repo object
            _countryRepository = _countryMockRepository.Object;
            _countryServices = new CountryServices(_countryRepository, _mocklogger.Object);
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
            CountryAddRequest countryAddRequest = null;

            //mock

            Country country= _fixture.Build<Country>().Without(temp => temp.Persons).Create(); // mock data 

            _countryMockRepository.Setup(x => x.AddCountry(It.IsAny<Country>()))
             .ReturnsAsync(country); // here its say when ever AddCountry is called with any country object return country object with auto fixture return data



            // assert and act together
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _countryServices.AddCountry(countryAddRequest);
            }
            );

        }

        [Fact]
        // check CountryName is not Null and not have space or special charcters in first
        public async Task AddCountry_NullCountryName()
        {
            // Arrange
            var countryAddRequest = _fixture.Build<CountryAddRequest>()
                .With(p => p.CountryName, null as string) // Explicitly setting to null
                .Create();

            // Mock repository (this will NOT be hit if validation fails first)
            var country = countryAddRequest.ToCountry();
            _countryMockRepository.Setup(x => x.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync(country);

            // Act & Assert using FluentAssertions
            Func<Task> action = async () => await _countryServices.AddCountry(countryAddRequest);

            await action.Should().ThrowAsync<ArgumentException>();


        }


        [Fact]
        public async Task AddCountry_DuplicateCountryName_ShouldThrowArgumentException()
        {
            // Arrange
            var duplicateCountryName = "Malasia";

            var countryAddRequests = new List<CountryAddRequest>
            {
                _fixture.Build<CountryAddRequest>().With(p => p.CountryName, duplicateCountryName).Create(),
                _fixture.Build<CountryAddRequest>().With(p => p.CountryName, duplicateCountryName).Create(),
                _fixture.Create<CountryAddRequest>()
            };

            var expectedCountry = countryAddRequests.First().ToCountry();

            // Mock repository: throw an exception if a duplicate country name is added

            _countryMockRepository
                .Setup(x => x.AddCountry(It.Is<Country>(temp => temp.CountryName == duplicateCountryName)))
                .ThrowsAsync(new ArgumentException("Country name already exists"));

                // Act
                Func<Task> action = async () =>
                {
                    foreach (var request in countryAddRequests)
                    {
                        await _countryServices.AddCountry(request);
                    }
                };

                // Assert using FluentAssertions
                await action.Should().ThrowAsync<ArgumentException>();
                
        }



        [Fact]
        // insert proper data
        public async Task AddCountry_AddCountryProperData()
        {
            //arrage
            var CountryAddRequest = _fixture.Build<CountryAddRequest>().Create();
            Country country = CountryAddRequest.ToCountry();
            CountryResponse countryExpectedList = country.ToCountryResponse();


            // mock


            // Mock setup
            _countryMockRepository.Setup(x => x.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync((Country c) =>
                {
                    c.CountryId = Guid.NewGuid(); // Ensure a new GUID is assigned in mock
                    return c;
                });



            //act
            var CountryAddedResponse = await _countryServices.AddCountry(CountryAddRequest);

            countryExpectedList.CountryId = CountryAddedResponse.CountryId; // add id to expected list as its only generated at run time

            //assert
            Assert.NotNull(CountryAddedResponse);

            //fluent assertion
            CountryAddedResponse.Should().BeEquivalentTo(countryExpectedList);

            Assert.True(CountryAddedResponse.CountryId != Guid.Empty);// ie checking some guid is generated

        }

        #endregion  AddCountry
    }

    
}
