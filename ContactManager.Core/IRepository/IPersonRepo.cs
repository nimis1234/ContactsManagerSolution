using DTO;
using DTO.Enums;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRepository
{
    public interface IPersonRepo
    {
        Task<List<Person?>> GetAllPersons();
        Task<Person?> GetPersonById(Guid? id);
        Task<Person?> AddPerson(Person person);
        Task<Person> UpdatePerson(Person Person);
        Task<bool> DeletePerson(Guid? id);
        Task<List<Person>> GetFilteredPersons(string? searchBy, string? searchString);
        List<Person> GetSortedData(List<Person> allPersons, string sortBy, SortOrderOption sortOrder);
       
    }
}
