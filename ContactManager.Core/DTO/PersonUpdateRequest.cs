﻿using DTO.Enums;
using Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public  class PersonUpdateRequest
    {
        [Required(ErrorMessage = "Person ID can't be blank")]
        public Guid PersonID { get; set; }

        [Required(ErrorMessage = "Person Name can't be blank")]
        public string? PersonName { get; set; }

        [Required(ErrorMessage = "Email can't be blank")]
        [EmailAddress(ErrorMessage = "Email value should be a valid email")]
        public string? Email { get; set; }

        public DateTime? DateOfBirth { get; set; }
        public GenderOptions? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }


        public Person ToPerson()
        {
            return new Person() { PersonID = PersonID, PersonName = PersonName, Email = Email, DateOfBirth = DateOfBirth, Gender = Gender.ToString(), Address = Address, CountryId = CountryID, ReceiveNewsLetters = ReceiveNewsLetters };
        }

        public bool Equals(PersonUpdateRequest other)
        {
            if (other == null) return false;

            return PersonName == other.PersonName &&
                   Email == other.Email &&
                   Gender == other.Gender &&
                   Address == other.Address &&
                   CountryID == other.CountryID &&
                   DateOfBirth == other.DateOfBirth &&
                   ReceiveNewsLetters == other.ReceiveNewsLetters;
        }


    }
}
