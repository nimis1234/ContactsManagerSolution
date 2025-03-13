using DTO.Enums;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PersonResponse
    {

     
        public Guid PersonID { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public double? Age { get; set; }  // here we add extra few more properties  to person response model to show the data to user ie perosn model does not these much proeprties


        public override string ToString()
        {
            return PersonName + " " + Email + " " + DateOfBirth + " " + Gender + " " + Country + " " + Address + " " + ReceiveNewsLetters;
        }


        public override bool Equals(object? obj)
        {
            if (obj == null || obj.GetType() != typeof(PersonResponse))
            {
                return false;
            }

            var personResponse = (PersonResponse)obj;

            return personResponse.PersonID == PersonID &&
                   personResponse.PersonName == PersonName &&
                   personResponse.Email == Email &&
                   personResponse.DateOfBirth == DateOfBirth &&
                   personResponse.Gender == Gender &&
                   personResponse.CountryID == CountryID &&
                   personResponse.Country == Country &&
                   personResponse.Address == Address &&
                   personResponse.ReceiveNewsLetters == ReceiveNewsLetters;
        }

        public PersonUpdateRequest ToUpdatePersonRequest()  // This method will convert person response  to person update request
        {
            return new PersonUpdateRequest()
            {
                PersonID = PersonID,
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth,
                Gender = (GenderOptions)Enum.Parse(typeof(GenderOptions), Gender,true), //used to convert geneder.tostring () to proper gender option
                CountryID = CountryID,
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters
            };
        }

    }


   

    public static class PersonExtensions  // EXTENSION METHOD WILL B INJECT TO PERSON MODEL
    {
        public static PersonResponse ToPersonResponse(this Person person) // this will determine this needs to inject to countRY model
        {
            if(person==null)
            {
                return null;
            }

            PersonResponse personResponse = new PersonResponse(); // converting PERSON model to counry response. This way model gets hiiden from the user model to person response. This way model gets hiiden from the user
            personResponse.PersonID = person.PersonID;
            personResponse.PersonName = person.PersonName;
            personResponse.Email = person.Email;
            personResponse.DateOfBirth = person.DateOfBirth;
            personResponse.Gender = person.Gender;
            personResponse.CountryID = person.CountryId;
            personResponse.Address = person.Address;
            personResponse.ReceiveNewsLetters = person.ReceiveNewsLetters;
            personResponse.Age = person.DateOfBirth.HasValue
                ? DateTime.Now.Year - person.DateOfBirth.Value.Year
                    - (DateTime.Now.Date < person.DateOfBirth.Value.Date.AddYears(DateTime.Now.Year - person.DateOfBirth.Value.Year) ? 1 : 0)
                : (double?)null;


            return personResponse;
        }
    }
}
