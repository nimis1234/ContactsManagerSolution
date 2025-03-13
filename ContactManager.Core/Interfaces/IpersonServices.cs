using DTO;
using DTO.Enums;
using Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IpersonServices
    {
        Task<List<PersonResponse?>> GetAllPersons();
        Task<PersonResponse?> GetPersonById(Guid? id);
        Task<PersonResponse?> AddPerson(PersonAddRequest person);
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest person);
        Task<bool>  DeletePerson(Guid? id);
        Task<List<PersonResponse>> GetFilteredPersons(string? searchBy, string? searchString);
        List<PersonResponse> GetSortedData(List<PersonResponse> allPersons, string sortBy, SortOrderOption sortOrder);
        Task<MemoryStream?> ExportToCSV(); // Add a method name to complete the interface definition
        Task<MemoryStream?> ExportToExcel();
    }
}
