using DTO;
using DTO.Enums;
using Entities;
using IRepository;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.Metrics;
using System.Linq.Expressions;

namespace Repository
{
    public class PersonRepo : IPersonRepo
    {
        private readonly ApplicationDbContext _dbContext;

        public PersonRepo(ApplicationDbContext dbContext)
        {
            _dbContext= dbContext;
        }
        public async Task<Person?> AddPerson(Person person)
        {
            if(person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }
            _dbContext.Persons.Add(person);
            await _dbContext.SaveChangesAsync();
            return person;
        }

        public async Task<bool> DeletePerson(Guid? id)
        {
            var personToDelete= new Person();
            try
            {
                if (id == null)
                {
                    throw new ArgumentNullException(nameof(id));
                }

                personToDelete = await _dbContext.Persons.FirstOrDefaultAsync(temp => temp.PersonID == id);
                if (personToDelete == null)
                {
                    return false; // Return false if person does not exist
                }

                _dbContext.Persons.Remove(personToDelete);
                var value = await _dbContext.SaveChangesAsync();
                return value > 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
           

        }


        public async Task<List<Person?>> GetAllPersons()
        {
            return await Task.Run(() => _dbContext.GetPersonListViaSP().Cast<Person?>().ToList());
        }

        public Task<List<Person>> GetFilteredPersons(string? searchBy, string? searchString)
        {
            throw new NotImplementedException();
        }

        public async Task<Person?> GetPersonById(Guid? id)
        {
            return await _dbContext.Persons.FirstOrDefaultAsync(temp => temp.PersonID == id);
        }

        public List<Person> GetSortedData(List<Person> allPersons, string sortBy, SortOrderOption sortOrder)
        {
            throw new NotImplementedException();
        }

        public async Task<Person> UpdatePerson(Person  person)
        {
          var personToUpdate =  _dbContext.Persons
              .Include("Country") // can include country since it is a navigation property
              .FirstOrDefault(temp => temp.PersonID== person.PersonID);
              if (personToUpdate == null)
              {
                throw new ArgumentException($"Person with id {person.PersonID} does not exist");
              }
              personToUpdate.PersonName = person.PersonName;
              personToUpdate.Email = person.Email;
              personToUpdate.Address = person.Address;
              personToUpdate.DateOfBirth = person.DateOfBirth;
              personToUpdate.Gender = person.Gender.ToString();
              personToUpdate.ReceiveNewsLetters = person.ReceiveNewsLetters;
              personToUpdate.CountryId = person.CountryId;
             await _dbContext.SaveChangesAsync(); //save changes to db
             return personToUpdate;
        }
    }
}
