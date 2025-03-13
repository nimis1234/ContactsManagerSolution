using AutoFixture;
using DTO;
using Entities;
using FluentAssertions;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CrudTest
{
    public class PersonControllerIntegrationTest
    {
        private readonly Fixture _fixture;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ApplicationDbContext dbContext { get; private set; }

        public PersonControllerIntegrationTest()
        {
            _fixture = new Fixture();
            _factory = new CustomWebApplicationFactory(); // Use custom factory
             _client = _factory.CreateClient();
            SeedTestData(); // Seed database in constructor since im calling stored procedure, In memory database is used , cannot invoke stored procedure so we need to seed data
        }

        private void SeedTestData()
        {
             using var scope = _factory.Services.CreateScope();
             var services = scope.ServiceProvider;
             dbContext = services.GetRequiredService<ApplicationDbContext>();

            // Only seed if the table is empty
            if (!dbContext.Persons.Any())
            {
                dbContext.Persons.AddRange(new List<Person>()
            {
                _fixture.Build<Person>().With(p => p.PersonID, Guid.NewGuid()).With(p => p.Email, "jane@example").With(p => p.Country, null as Country).Create(),
                _fixture.Build<Person>().With(p => p.PersonID, Guid.NewGuid()).With(p => p.Email, "john@example").With(p => p.Country, null as Country).Create(),
            });
                dbContext.SaveChanges();
            }
            if (!dbContext.Countries.Any())
            {
                dbContext.Countries.AddRange(new List<Country>()
               {
                _fixture.Build<Country>().Without(temp => temp.Persons).Create(),
                _fixture.Build<Country>().Without(temp => temp.Persons).Create()
                });
               dbContext.SaveChanges();
            }

        }


        [Fact]
        public async Task Index_ReturnsAViewResult_WithAListOfPersons()
        {
            HttpResponseMessage response = await _client.GetAsync("/Persons/Index"); //check the status code
                                                                                     //fluent assertion 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            //check the content type
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Should().NotBeNullOrEmpty();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(responseBody);
            var document = html.DocumentNode; // get the root node of the document
                                              //now check Persons table is present or not
            document.SelectNodes("//table[contains(@class, 'persontable')]").Should().NotBeNull(); //MEAN TABLE CONTINATES PERSONS CLASS OR NOT
        }


        [Fact]
        public async Task Create_ViewReturnsWithSucess()
        {
            HttpResponseMessage response = await _client.GetAsync("/Persons/CreatePerson"); //check the status code
                                                                                     //fluent assertion 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);


            //check the content type
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Should().NotBeNullOrEmpty();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(responseBody);
            var document = html.DocumentNode; // get the root node of the document
                                              //now check Persons table is present or not
           // document.SelectNodes("//table[contains(@class, 'persontable')]").Should().NotBeNull(); //MEAN TABLE CONTINATES PERSONS CLASS OR NOT
        }

        /*[Fact]
        public async Task Create_ViewReturnsWithError()
        {
            // add error data
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
               .With(p => p.Email, "testcom").Create(); // if 

            var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(personAddRequest),
                                    System.Text.Encoding.UTF8, "application/json");
            HttpResponseMessage response = await _client.PostAsync("/Persons/CreatePerson", content);
            //fluent assertion 
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

            // read response body
            var responseBody = await response.Content.ReadAsStringAsync();
            responseBody.Should().NotBeNullOrEmpty();
            HtmlDocument html = new HtmlDocument();
            html.LoadHtml(responseBody);
            var document = html.DocumentNode;

            // Check if ViewBag.Errors are displayed in the view
            var errorNode = document.SelectSingleNode("//div[@id='validationError']"); // // Select the div with id="error-messages"
            errorNode.Should().NotBeNull(); // Ensure error messages exist
            errorNode.InnerText.Should().Contain("Email is required"); // Ensure the error message contains "Invalid email"
       
        } */
    }
}
