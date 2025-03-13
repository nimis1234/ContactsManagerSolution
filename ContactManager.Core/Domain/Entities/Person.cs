using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Person model
    /// </summary>
    public class Person
    {
        [Key]
        public Guid PersonID { get; set; }
        [StringLength(50)]
        public string? PersonName { get; set; }
        [StringLength(50)]
        public string? Email { get; set; }
        [StringLength(50)]
        public DateTime? DateOfBirth { get; set; }
        [StringLength(7)]
        public string? Gender { get; set; }

  
        [StringLength(50)]
        public Guid? CountryId { get; set; }
        [StringLength(50)]
        public string? Address { get; set; }
        [StringLength(5)]
        public bool ReceiveNewsLetters { get; set; }

        [StringLength(15)]
        public string? TIN{ get; set; }


        [ForeignKey("CountryId")] // foreign key attribute
        public Country? Country { get; set; } // navigation property to Country here Country is Parent and Person is child
       //MANY TO ONE ie one country can have many person

    }
}
