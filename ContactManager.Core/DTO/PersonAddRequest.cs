using DTO.Enums;
using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PersonAddRequest
    {
        [Required (ErrorMessage = "Person Name is required")]

        public string? PersonName { get; set; }


        [Required (ErrorMessage = "Email is required")]
        [EmailAddress (ErrorMessage = "Invalid email address")]
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public GenderOptions? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }

        


        public Person ToPerson()
        {
            if (PersonName == null)
            {
                throw new ArgumentNullException(nameof(PersonName));
            }

            Person person = new Person() // this is a method for converting person add request to person model for saving to db
            {
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth,
                Gender = Gender.ToString(),
                CountryId = CountryID,
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters
            };
            return person;
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
    }
}
