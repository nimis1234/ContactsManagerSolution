using AutoFixture;
using Castle.Core.Logging;
using CURDExample.Controllers;
using DTO;
using DTO.Enums;
using FluentAssertions;
using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
/*
  for tetsing controller mocked Services
 */
namespace CrudTest
{
    public class PersonalControllerTest
    {
        private readonly Mock<IcountryServices> _countryServiceMock;
        private Mock<IpersonServices> _personsServiceMock;
        private readonly Mock<ILogger<PersonServices>> _mockLogger;
        private readonly Mock<ILogger<PersonsController>> _mockLoggerController;
        private readonly IcountryServices _countryService;
        private readonly IpersonServices _personService;
        private readonly ILogger<PersonServices> _mockLoggerObject;
        private readonly Fixture _fixture;

        public PersonalControllerTest()
        {
            _countryServiceMock= new Mock<IcountryServices>();
            _personsServiceMock = new Mock<IpersonServices>();

            _mockLogger = new Mock<ILogger<PersonServices>>();

            _mockLoggerController = new Mock<ILogger<PersonsController>>();

            _countryService = _countryServiceMock.Object;
            _personService = _personsServiceMock.Object;
            _mockLoggerObject = _mockLogger.Object;
            _fixture = new Fixture();
        }


        // index controller should return with personlist
        [Fact]
        public async Task Index_ShouldReturnIndexViewWithPersonsList()
        {
            // Arrange
            List<PersonResponse> personResponses = _fixture.CreateMany<PersonResponse>().ToList();
            PersonsController personsController = new PersonsController(_countryService, _personService, _mockLoggerController.Object);

            //now mock service saying that service return the personResponses
            // 3 methods are defined in personServices 
            _personsServiceMock.Setup(temp => temp.GetAllPersons()).ReturnsAsync(personResponses);

            _personsServiceMock
                .Setup(temp => temp.GetFilteredPersons(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(personResponses);

            _personsServiceMock
            .Setup(temp => temp.GetSortedData(It.IsAny<List<PersonResponse>>(), It.IsAny<string>(), It.IsAny<SortOrderOption>()))
            .Returns(personResponses);

            // now call controller Index method
            IActionResult result = await personsController.Index(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);// chcking return is viewResult
            var model = Assert.IsAssignableFrom<IEnumerable<PersonResponse>>(viewResult.ViewData.Model); // checking return is IEnumerable personresponsemodal
            Assert.Equal(personResponses.Count, model.Count());
        }

        [Fact]
        public async Task CreatePersonView_GetSucess()
        {
            // Arrange
            PersonsController personsController = new PersonsController(_countryService, _personService, _mockLoggerController.Object);

            //mock service
            _countryServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(_fixture.CreateMany<CountryResponse>().ToList());


            // Act
            var result = await personsController.CreatePerson();
            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);           
        }

        [Fact]
        public async Task CreatePersonView_PostSuccess()
        {
            var PersonAddRequest = _fixture.Build<PersonAddRequest>().With(p => p.Email, "test@gmail.com").Create();
            PersonResponse personResponse= PersonAddRequest.ToPerson().ToPersonResponse();
            // Arrange
            PersonsController personsController = new PersonsController(_countryService, _personService, _mockLoggerController.Object);
            _countryServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(_fixture.CreateMany<CountryResponse>().ToList());
            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
            .ReturnsAsync(personResponse); // Mocking the async method

            IActionResult result = await personsController.CreatePerson(PersonAddRequest);
            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            // check both view AND MODAL

            Assert.Equal("Index", redirectResult.ActionName);
            Assert.Equal("Persons", redirectResult.ControllerName);

            // Verify that the AddPerson method was called exactly once
            _personsServiceMock.Verify(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()), Times.Once);
        }


        [Fact]
        public async Task CreatePerson_PostInvalidModel()
        {
            var PersonAddRequest = _fixture.Build<PersonAddRequest>().Create();
            PersonResponse personResponse = PersonAddRequest.ToPerson().ToPersonResponse();
            var countryResponses = _fixture.CreateMany<CountryResponse>().ToList();
            // Arrange
            PersonsController personsController = new PersonsController(_countryService, _personService, _mockLoggerController.Object);
            _countryServiceMock.Setup(temp => temp.GetAllCountries()).ReturnsAsync(countryResponses);
            _personsServiceMock.Setup(temp => temp.AddPerson(It.IsAny<PersonAddRequest>()))
            .ReturnsAsync(personResponse); // Mocking the async method

            // inject modal error
            personsController.ModelState.AddModelError("PersonName", "Person Name can't be blank");

            // Act
            var result = await personsController.CreatePerson(PersonAddRequest);

            // Ensure ViewBag.Countries is set
            var viewBagCountries = Assert.IsAssignableFrom<List<SelectListItem>>(personsController.ViewBag.Countries);
            Assert.NotNull(viewBagCountries);
            Assert.Equal(countryResponses.Count, viewBagCountries.Count);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            

        }
    }
}
