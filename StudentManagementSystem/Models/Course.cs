using System.Collections.Generic;

namespace StudentManagementSystem.Models
{
    public class CourseCode
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int Credits { get; set; }
        public string Description { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }
}