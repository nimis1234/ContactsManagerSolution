using DTO;
using DTO.Enums;
using Entities;
using EntityFrameworkCore.Testing.Moq;
using Interfaces;
using Microsoft.EntityFrameworkCore;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
using Moq;
using EntityFrameworkCoreMock;

namespace CrudTest
{
    /*
    public class StoredProcedureTest :IDisposable
    {
        private readonly PersonServices _personServices;
        private readonly CountryServices _countryServices;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly DbContextMock<ApplicationDbContext> _dbContextMock;

        public StoredProcedureTest(ITestOutputHelper testOutputHelper)
        {
            var countriesInitialData = new List<Country>();
            var personsInitialData = new List<Person>();

            // Create DbContext mock
            _dbContextMock = new DbContextMock<ApplicationDbContext>(
                new DbContextOptionsBuilder<ApplicationDbContext>().Options
            );

            // Mock DbSets
            _dbContextMock.CreateDbSetMock(x => x.Countries, countriesInitialData);
            _dbContextMock.CreateDbSetMock(x => x.Persons, personsInitialData);

            // Mock stored procedure methods
            var dbContext = _dbContextMock.Object;

            // 1. Mock GetPersonListViaSP to return in-memory Persons
            _dbContextMock.Setup(x => x.GetPersonListViaSP())
                .Returns(() => dbContext.Persons.ToList());

            // 2. Mock InsertCountryNameViaSP to add to in-memory Countries
            _dbContextMock.Setup(x => x.InsertCountryNameViaSP(
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

            // Initialize services
            _countryServices = new CountryServices(dbContext);
            _personServices = new PersonServices(dbContext, _countryServices);

            _testOutputHelper = testOutputHelper;
        }

        public void Dispose()
        {
            _dbContextMock.Object.Dispose();
        }

        [Fact]
        public async Task DeletePerson_ValidPersonId()
        {
            // Arrange
            var countryAddRequest1 = new CountryAddRequest { CountryName = "Iia" };
            var countryAddRequest2 = new CountryAddRequest { CountryName = "Kuwait" };

            // These will use the mocked InsertCountryNameViaSP
            var countryResponse1 = _countryServices.AddCountry(countryAddRequest1);
            var countryResponse2 = _countryServices.AddCountry(countryAddRequest2);

            var personAddRequest1 = new PersonAddRequest
            {
                PersonName = "Smith",
                Email = "smith@example.com",
                Gender = GenderOptions.Male,
                CountryID = countryResponse1.CountryId
            };

            var personAddRequest2 = new PersonAddRequest
            {
                PersonName = "Mary",
                Email = "mary@example.com",
                Gender = GenderOptions.Female,
                CountryID = countryResponse2.CountryId
            };

            // Add to mocked Persons DbSet
            var personResponse1 = await _personServices.AddPerson(personAddRequest1);
            var personResponse2 = await _personServices.AddPerson(personAddRequest2);

            // Act
            _personServices.DeletePerson(personResponse1.PersonID);

            // Assert
            // Uses mocked GetPersonListViaSP
            var allPersons = await _personServices.GetAllPersons();
            Assert.DoesNotContain(allPersons, p => p.PersonID == personResponse1.PersonID);
        }
    }

    */
}