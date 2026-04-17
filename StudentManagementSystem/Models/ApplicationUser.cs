using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace StudentManagementSystem.Models
{
    // ApplicationUser with extended properties
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [Display(Name = "Full Name")]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 3)]
        public string FullName { get; set; }

        [Display(Name = "Student ID")]
        public int? StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [Display(Name = "Profile Picture")]
        public string ProfilePicturePath { get; set; }

        [NotMapped]
        [Display(Name = "Profile Picture")]
        public HttpPostedFileBase ProfilePictureFile { get; set; }

        [Display(Name = "Date of Birth")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Address")]
        [StringLength(200)]
        public string Address { get; set; }

        // ✅ This method solves the error
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note: authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            // Add custom user claims here
            return userIdentity;
        }
    }

    // ApplicationDbContext with custom entities
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Student> Students { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Grade> Grades { get; set; }              
        public DbSet<Announcement> Announcements { get; set; } 

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Enrollment>()
                .HasRequired(e => e.Student)
                .WithMany(s => s.Enrollments)
                .HasForeignKey(e => e.StudentId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Enrollment>()
                .HasRequired(e => e.Course)
                .WithMany(c => c.Enrollments)
                .HasForeignKey(e => e.CourseId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Enrollment>()
                .HasOptional(e => e.Grade)
                .WithRequired(g => g.Enrollment);
        }
    }
}
