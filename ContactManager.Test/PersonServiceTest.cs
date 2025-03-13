using AutoFixture;
using Castle.Core.Logging;
using DTO;
using DTO.Enums;
using Entities;
using EntityFrameworkCoreMock;
using FluentAssertions;
using FluentAssertions.Execution;
using Interfaces;
using IRepository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Repository;
using Services;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;
/*
 
 for testing services , mocked repo 
 
 */


namespace CrudTest
{

    // few things to remember only add one method or function call in each test case
    public class PersonServiceTest 
    {
        private readonly Fixture _fixture;
        private readonly IPersonRepo _personRepository;
        private readonly CountryServices _counryService;
        private PersonServices _personService;
        private readonly ITestOutputHelper _testHelper;
        private readonly Mock<IPersonRepo> _personRepositoryMock;
        private Mock<ICountryRepo> _countryRepositoryMock;
        private readonly Mock<ILogger<PersonServices>> _mockPersonServiceLogger;
        private readonly Mock<ILogger<CountryServices>> _mockCountryServiceLogger;
        private ICountryRepo _countryRepository;
        private DbContextMock<ApplicationDbContext> dbContextMock;

        public PersonServiceTest(ITestOutputHelper testOutputHelper)
        {
            _fixture = new Fixture();
            var countriesInitialData = new List<Country>();
            var personsInitialData = new List<Person>();
            //mock repo
            _personRepositoryMock = new Mock<IPersonRepo>();
            _countryRepositoryMock = new Mock<ICountryRepo>();

            //mock logger
            _mockPersonServiceLogger = new Mock<ILogger<PersonServices>>();
            _mockCountryServiceLogger = new Mock<ILogger<CountryServices>>();

            // create mock repo object
            _personRepository = _personRepositoryMock.Object;
            _countryRepository = _countryRepositoryMock.Object;

            _counryService = new CountryServices(_countryRepository, _mockCountryServiceLogger.Object);
            _personService = new PersonServices(_personRepository, _counryService, _mockPersonServiceLogger.Object);
            _testHelper = testOutputHelper;

        }

        #region AddPerson PersonAddRequestNull
        [Fact]
        public async Task AddPerson_PersonAddRequestNull()
        {
            // Arrange
            PersonAddRequest? personAddRequest = null;
            Person person = personAddRequest?.ToPerson();

            //mock repo
            _personRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>())).ReturnsAsync(person);

            // assert and act together
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var personResponse = await _personService.AddPerson(personAddRequest);
                if(personResponse == null)
                {
                    throw new ArgumentNullException(nameof(personResponse));
                }
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
            Person person = personAddRequest.ToPerson();
            // we are mocking the repo and setup the mock
            _personRepositoryMock.Setup
             (temp => temp.AddPerson(It.IsAny<Person>()))
             .ReturnsAsync(person);

            PersonResponse expectedPersonResponse = person.ToPersonResponse();

            //act
            PersonResponse personResponseAdded = await _personService.AddPerson(personAddRequest);

            // we can add same Id to personResponseExpected
            expectedPersonResponse.PersonID = personResponseAdded.PersonID;

            //now check both are same or not

            //FluentAssertions
            personResponseAdded.Should().NotBeNull();
            // Compare individual properties or use should.be ether one of them
            personResponseAdded.PersonID.Should().Be(expectedPersonResponse.PersonID);
            personResponseAdded.PersonName.Should().Be(expectedPersonResponse.PersonName);
            personResponseAdded.Email.Should().Be(expectedPersonResponse.Email);
            personResponseAdded.DateOfBirth.Should().Be(expectedPersonResponse.DateOfBirth);
            personResponseAdded.Gender.Should().Be(expectedPersonResponse.Gender);
            personResponseAdded.Address.Should().Be(expectedPersonResponse.Address);
            personResponseAdded.CountryID.Should().Be(expectedPersonResponse.CountryID);
            personResponseAdded.ReceiveNewsLetters.Should().Be(expectedPersonResponse.ReceiveNewsLetters);

           // personResponseAdded.Should().Be(expectedPersonResponse);
        }
        #endregion


        #region model validation


        [Fact]
        public void AddPerson_PersonModelIsNotValid()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.Email, "testgmaicom").Create();

            //Act
            ModelValidation validatePerson = new ModelValidation();
            var validationResults = new List<ValidationResult>();
            bool isValid = ModelValidation.ValidateModel(personAddRequest, out validationResults);
            //Assert
            Assert.False(isValid);
        }
        #endregion


        //If we supply null as PersonID, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
        {
            //Arrange
            Guid? personID = null;

            //mock
            _personRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>())).ReturnsAsync(null as Person);

            //Act
            PersonResponse? person_response_from_get = await _personService.GetPersonById(personID);


            //Assert
            person_response_from_get.Should().BeNull(); //using fluent assertion instead of normal assert
        }


        #region GetAllPerson
        [Fact]
        public async Task GetAllPerson_GetAllPersonToBeSuccess()
        {
            // first add countries
            //Arrange
            List<PersonAddRequest> personsAddRequest = new List<PersonAddRequest>() {
            _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone_1@example.com")
            .Create(),

            _fixture.Build<PersonAddRequest>()
            .With(temp => temp.Email, "someone_2@example.com")
            .Create(),

            _fixture.Build<PersonAddRequest>()
            .Create()
           };

            var persons = personsAddRequest.Select(temp => temp.ToPerson()).ToList(); //convert to person response 

            List<PersonResponse> expectedPersonResponse = persons.Select(temp => temp.ToPersonResponse()).ToList();
            // mock 

            _personRepositoryMock.Setup(temp => temp.GetAllPersons())
             .ReturnsAsync(persons);


            //Act
            List<PersonResponse?> personsListFromRepo = await _personService.GetAllPersons();


            //Fluent Assertion
            personsListFromRepo.Should().NotBeNull();
            personsListFromRepo.Should().BeEquivalentTo(expectedPersonResponse);
        }
        #endregion

        [Fact]
        public async Task GetPerson_GetFilteredPersons_SortBy()
        {
            //Arange

            //Arrange
            List<Person> persons = new List<Person>() {
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


            var expectedPersonAddList = persons.Select(temp => temp.ToPersonResponse()).ToList(); //convert to person response 

            _personRepositoryMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(persons);


            //Act
            List<PersonResponse?> getAllPersons = await _personService.GetAllPersons();

            var getFilteredPersons = _personService.GetSortedData(getAllPersons, nameof(PersonResponse.PersonName), SortOrderOption.DESC);



            /*
                       for (int i = 0; i < getFilteredPersons.Count(); i++)
                       {
                           // compare sorted list 
                           Assert.Equal(getFilteredPersons[i].PersonName, ExpectedList[i].PersonName);
                       }
            */

            //fluent assertion
            getFilteredPersons.Should().BeEquivalentTo(expectedPersonAddList);

        }



        #region DeletePerson
        //If you supply an invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {

            _personRepositoryMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>())).ReturnsAsync(false); // menas going to call DeletePerson passing a guid as paramter and return value true

            //Act
            bool isDeleted = await _personService.DeletePerson(Guid.Empty);

            //Assert
            isDeleted.Should().BeFalse();
        }
        #endregion



        #region DeletePerson valid
        [Fact]
        public async Task DeletePerson_ValidPersonID_ToBeSuccessful()
        {
            //Arrange
            _personRepositoryMock.Setup(temp => temp.DeletePerson(It.IsAny<Guid>())).ReturnsAsync(true); // menas going to call DeletePerson passing a guid as paramter and return value true

            //Act
            bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());
            isDeleted.Should().BeTrue();
        }

        #endregion
        //First, add a new person and try to update the person name and email

        #region UpdatePerson

        [Fact]
        public async Task UpdatePerson_PersonFullDetails_ToBeSuccessful()
        {
            //Arrange
            PersonUpdateRequest personUpdateRequest = _fixture.Build<PersonUpdateRequest>()
             .With(temp => temp.Email, "someone@example.com")
             .With(temp => temp.CountryID, null as Guid?)
             .With(temp => temp.Gender, GenderOptions.Male)
             .Create();

            Person person = personUpdateRequest.ToPerson();

            PersonResponse person_response_expected = person.ToPersonResponse();


            _personRepositoryMock
             .Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
             .ReturnsAsync(person);

            _personRepositoryMock
             .Setup(temp => temp.GetPersonById(It.IsAny<Guid>()))
             .ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_update = await _personService.UpdatePerson(personUpdateRequest);

            //Assert
            person_response_from_update.Should().Be(person_response_expected);
        }

     
        #endregion



    } // END OF CLASS


} // END OF NAMESPACE 
