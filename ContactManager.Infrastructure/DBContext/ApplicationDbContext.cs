
using ContactManager.Core.Domain.IdentityEntity;
using Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System.Data;

namespace Entities
{
    // please install EntityFrameworkCore.SqlServer to enities library
  
    //public class ApplicationDbContext : DbContext // instead of dbcontext, we can use identitydbcontext as well for identity 
      public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid> // instead of dbcontext, we can use identitydbcontext as well for identity, and passing id datatype as guid
    {
        public bool IsInMemory { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            if (!options.Extensions.OfType<InMemoryOptionsExtension>().Any())
            {
                IsInMemory = false;
            }
            else
            {
                IsInMemory = true;
            }
        }

       

        public virtual DbSet<Person> Persons { get; set; } // add as virtual  so can help to mock the data for testing
        public virtual DbSet<Country> Countries { get; set; }

        // for stored procedure
        public virtual int InsertCountryNameViaSP(string countryName, Guid countryId)
        {
            SqlParameter[] parameters = new SqlParameter[]
            {
                new SqlParameter("@CountryName", countryName),
                new SqlParameter("@CountryId", countryId)
            };  
           
            return Database.ExecuteSqlRaw("sp_InsertCountryName @CountryName, @CountryId", parameters); // this is how stored procedure called
        }

        // for getting person list

        public virtual List<Person> GetPersonListViaSP()
        {
            try
            {
                if (IsInMemory)
                {
                    // Simulate stored procedure behavior using LINQ for in-memory database
                    return Persons.ToList();
                }
                else
                {
                    // Use raw SQL for other providers
                    return Persons.FromSqlRaw("EXEC sp_GetAllPersons").ToList();
                }
            }
            catch (CustomExceptions ex)
            {
                throw new CustomExceptions("An error occurred while executing the stored procedure.", ex);
            }
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>().ToTable("Persons");
            modelBuilder.Entity<Country>().ToTable("Countries");

            // read json file and map to model
            //fill some data to the database
            //this only for inital time 

            /*
               IdentityUserRole<Guid> requires a primary key to be defined
                IdentityUserLogin<Guid> requires a primary key to be defined
             
             */

            modelBuilder.Entity<IdentityUserLogin<Guid>>(builder =>
            {
                builder.HasKey(l => new { l.LoginProvider, l.ProviderKey });
            });


            modelBuilder.Entity<IdentityUserRole<Guid>>(builder =>
            {
                builder.HasKey(ur => new { ur.UserId, ur.RoleId });
            });


            modelBuilder.Entity<IdentityUserToken<Guid>>(builder =>
            {
                builder.HasKey(ut => new { ut.UserId, ut.LoginProvider, ut.Name });
            });

            var currentDirectory = Directory.GetCurrentDirectory();

            // Navigate up until we reach the CrudTest project directory
            //var projectDirectory = Directory.GetParent(currentDirectory).Parent.Parent.FullName;


            var fakepath = @"D:\ASPNETCORE\\ContactsManagerSolution\\ContactsManager.UI";

            // Assuming you want to put your files in a folder named 'TestResources' inside the 'CrudTest' project
            string countriesFilePath = Path.Combine(fakepath, "TestJson", "countries.json");
            string personsFilePath = Path.Combine(fakepath, "TestJson", "persons.json");

  



            string countriesJson = System.IO.File.ReadAllText(countriesFilePath);
            List<Country>? countries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesJson);

            string personJson = System.IO.File.ReadAllText(personsFilePath);
            List<Person>? persons = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personJson);

            if (countries != null)
            {
                foreach (var country in countries)
                {
                    modelBuilder.Entity<Country>().HasData(country);
                }
            }

            if (persons != null)
            {
                foreach (var person in persons)
                {
                    modelBuilder.Entity<Person>().HasData(person);
                }
            }

            //changing the column type default from nvarchar to varchar
            modelBuilder.Entity<Person>().
                Property(p => p.TIN)
                .HasColumnName("TaxIdentifierNumber")//can change the column name
                .HasColumnType("varchar(50)");

            // adding constraint
            modelBuilder.Entity<Person>()
             .ToTable(t => t.HasCheckConstraint( "CK_Person_Email_AtSymbol","Email LIKE '%@%'"))
             .Property(p => p.Email)
             .HasMaxLength(50);

        }
    }
}