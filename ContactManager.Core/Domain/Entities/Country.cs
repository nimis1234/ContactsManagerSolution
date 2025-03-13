using System.ComponentModel.DataAnnotations;

namespace Entities
{
    public class Country
    {
        /// <summary>
        /// The unique identifier for the entity.
        /// </summary>
        [Key]
        public Guid CountryId { get; set; }

        [StringLength(50)]
        public string? CountryName { get; set; }

        public virtual ICollection<Person> Persons { get; set; } = new List<Person>(); // navigation property to Person here Person is child and Country is Parent
    }
}
