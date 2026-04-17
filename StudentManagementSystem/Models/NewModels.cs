using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StudentManagementSystem.Models
{
    // ================= COURSE =================
    public class Course
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Course code is required")]
        [StringLength(10)]
        [Display(Name = "Course Code")]
        public string CourseCode { get; set; }

        [Required(ErrorMessage = "Course name is required")]
        [StringLength(100)]
        [Display(Name = "Course Name")]
        public string CourseName { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Credit Hours")]
        [Range(1, 6)]
        public int CreditHours { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Instructor")]
        public string Instructor { get; set; }

        public virtual ICollection<Enrollment> Enrollments { get; set; }
    }

    // ================= ENROLLMENT =================
    public class Enrollment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StudentId { get; set; }

        [Required]
        public int CourseId { get; set; }

        [Display(Name = "Enrollment Date")]
        public DateTime EnrollmentDate { get; set; }

        public virtual Student Student { get; set; }
        public virtual Course Course { get; set; }

        // ✅ One-to-One Relationship
        public virtual Grade Grade { get; set; }
    }

    // ================= GRADE =================
    public class Grade
    {
        // ✅ PRIMARY KEY + FOREIGN KEY (IMPORTANT FIX)
        [Key, ForeignKey("Enrollment")]
        public int EnrollmentId { get; set; }

        [Required]
        [Range(0, 100, ErrorMessage = "Score must be between 0 and 100")]
        [Display(Name = "Score (%)")]
        public decimal Score { get; set; }

        [StringLength(2)]
        [Display(Name = "Letter Grade")]
        public string LetterGrade { get; set; }

        [StringLength(200)]
        [Display(Name = "Remarks")]
        public string Remarks { get; set; }

        [Display(Name = "Date Assigned")]
        public DateTime DateAssigned { get; set; }

        public virtual Enrollment Enrollment { get; set; }
    }

    // ================= ANNOUNCEMENT =================
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(150)]
        public string Title { get; set; }

        [Required(ErrorMessage = "Body is required")]
        [StringLength(1000)]
        public string Body { get; set; }

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; }

        [StringLength(50)]
        public string Category { get; set; }
    }
}