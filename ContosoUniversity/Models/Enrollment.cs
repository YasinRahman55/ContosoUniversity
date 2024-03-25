using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace ContosoUniversity.Models
{
    //we create Enum in the namespace not inside the class
    public enum Grade
    {
        A, B, C, D, E, F
    }
    public class Enrollment
    {
        //PK (mn esm el class)
        public int EnrollmentID { get; set; }
        //FK
        public int CourseID { get; set; }
        //FK
        public int StudentID { get; set; }
        [DisplayFormat(NullDisplayText = "No grade")]
        //default null we added ?
        public Grade? Grade { get; set; }

        //navigation properties
        public Student Student { get; set; }
        public Course Course { get; set; }

    }
}
