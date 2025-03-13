using AutoFixture;
using DTO;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;

using IRepository;
using Microsoft.EntityFrameworkCore;
using Moq;
using Repository;
using Xunit;
using Xunit.Abstractions;

/*
 
 for testing repo , mocked db context 
 
 */

namespace CrudTest
{

    // few things to remember only add one method or function call in each test case
    public class PersonRepoTest :IDisposable
    {
        private readonly Fixture _fixture;
        private readonly IPersonRepo _personRepository;
        private readonly ITestOutputHelper _testHelper;
        private readonly Mock<IPersonRepo> _personRepositoryMock;
        private Mock<ICountryRepo> _countryRepositoryMock;
        private ICountryRepo _countryRepository;
        private readonly List<Person> personsInitialData;
        private DbContextMock<ApplicationDbContext> dbContextMock;

        public PersonRepoTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            var countriesInitialData = new List<Country>();
            var existingPerson = _fixture.Build<Person>()
                .With(p => p.PersonID, Guid.NewGuid()) // Ensure PersonID is set
                .With(p => p.Email, "someone@example.com")
                .With(p => p.Country, null as Country) // Ensure consistency with your setup
                .Create();

             personsInitialData = new List<Person> { existingPerson };

            // database mock 

            //Craete mock for DbContext
            dbContextMock = new DbContextMock<ApplicationDbContext>(
            new DbContextOptionsBuilder<ApplicationDbContext>().Options
             );

            ApplicationDbContext dbContext = dbContextMock.Object;

            //Create mocks for DbSets'
            dbContextMock.CreateDbSetMock(temp => temp.Countries, countriesInitialData);
            dbContextMock.CreateDbSetMock(temp => temp.Persons, personsInitialData);



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

            dbContextMock.Setup(x => x.GetPersonListViaSP())
                .Returns(() => dbContext.Persons.ToList()); // RETURN is crucial here as mock data getting after insertion via procedure

            _personRepository = new PersonRepo(dbContext);
            
            _testHelper = testOutputHelper;

        }

        #region AddPerson PersonAddRequestNull
        [Fact]
        public async Task AddPerson_PersonAddRequestNull()
        {

            // assert and act together
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await _personRepository.AddPerson(null);
            });
        }
        #endregion


        #region AddPerson PersonAddRequestProperData
        [Fact]
        public async Task AddPerson_PersonAddRequestProperData()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
              .With(p => p.Email, "test@gmail.com").Create(); // if we wanna customize the value use build.with

            // now convert to person
            Person personExpected = personAddRequest.ToPerson();


            //act
            var personResponseAdded = await _personRepository.AddPerson(personExpected);

            // we can add same Id to personResponseExpected
            personExpected.PersonID = personResponseAdded.PersonID;

            //now check both are same or not

          //FluentAssertions
            personResponseAdded.Should().NotBeNull();
            personResponseAdded.Should().Be(personExpected);

        }
        #endregion


        //If we supply null as PersonID, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            //Arrange
            Guid? personID = null;

            //Act
            Person? personGetByPersonID = await _personRepository.GetPersonById(personID);

            PersonResponse? person_response_from_get = personGetByPersonID?.ToPersonResponse();

            //Assert
            person_response_from_get.Should().BeNull(); //using fluent assertion instead of normal assert
        }


        #region GetAllPerson
        [Fact]
        public async Task GetAllPerson_GetAllPersonToBeSuccess()
        {
            // first add countries
            //Arrange
            List<Person> personsList = new List<Person>() {
            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone_1@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone_2@example.com")
            .With(temp => temp.Country, null as Country)
            .Create(),

            _fixture.Build<Person>()
            .With(temp => temp.Email, "someone_3@example.com")
            .With(temp => temp.Country, null as Country)
            .Create()
           };

            //mock db context
            dbContextMock.Setup(x => x.GetPersonListViaSP()) //mocked sp to return data
               .Returns(() => personsList);

            //Act
            List<Person?> personsListFromRepo = await _personRepository.GetAllPersons();


            //Fluent Assertion
            personsListFromRepo.Should().BeEquivalentTo(personsList);
        }
        #endregion




        #region DeletePerson
        //If you supply an invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {
            //Act
            bool isDeleted = await _personRepository.DeletePerson(Guid.NewGuid());

            //Assert
            isDeleted.Should().BeFalse();
        }
        #endregion



        #region DeletePerson valid
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            //Arrange
            // create a person and add it to the repository first to simulate an existing person
            var person = _fixture.Build<Person>()
                                 .With(p => p.PersonID, Guid.NewGuid())
                                  .With(temp => temp.Country, null as Country)// Assigning a new Guid
                                 .Create();

            
            
            var personAdded = await _personRepository.AddPerson(person);  // Add the person to the repository
            //Act
            // Mock the SaveChangesAsync method to return 1 when changes are saved
            dbContextMock.Setup(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()))
                         .ReturnsAsync(1);  // Indicating success
            var isDeleted = await _personRepository.DeletePerson(personAdded.PersonID);
            isDeleted.Should().BeTrue();
        }

        #endregion
        //First, add a new person and try to update the person name and email

    # region UpdatePerson

        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
        {
            //Arrange
            // get from personIniitalData
            Person personRequest = personsInitialData[0];

             //Act
            Person person_response_from_update = await _personRepository.UpdatePerson(personRequest);

            //Assert
            person_response_from_update.Should().BeEquivalentTo(personRequest);
        }

        public void Dispose()
        {
            // dbContextMock.Object.Dispose();
        }

        #endregion



    } // END OF CLASS


} // END OF NAMESPACE 
