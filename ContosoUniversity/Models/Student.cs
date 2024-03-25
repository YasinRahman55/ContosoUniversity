using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ContosoUniversity.Models
{
    //Base Model
    //Class -- table
    public class Student
    {
        // properties  -- columns
        //PK 
        public int ID { get; set; }
        // to make it null we add ? after the Datatype
        [Display(Name = "Last Name")]
        [Required(ErrorMessage ="Last Name is required.")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Z]+[a-zA-Z]*$", ErrorMessage = "Last Name must be letters only starting with Upper Letter.")]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        [Column("FirstName")]
        [RegularExpression(@"^[A-Z]+[a-zA-Z]*$", ErrorMessage = "First Name must be letters only starting with Upper Letter.")]
        
        public string FirstMidName { get; set; }

        
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        [Display(Name = "Full Name")]
        public string FullName
        {
            get
            {
                return LastName + ", " + FirstMidName;
            }
        }

        // Navigation properties
        public ICollection<Enrollment> Enrollments { get; set; }

    }
}
