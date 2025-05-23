using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using StudentManagementSystem.Models;
using System.Data.Entity;


namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Student/Profile
        public ActionResult Profile()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);

            if (user.StudentId == null)
            {
                return HttpNotFound();
            }

            var student = db.Students.Find(user.StudentId);
            return View(student);
        }

        // GET: Student/MyCourses
        public ActionResult MyCourses()
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);

            if (user.StudentId == null)
            {
                return HttpNotFound();
            }

            var student = db.Students
      .Include("Enrollments.Course")
      .FirstOrDefault(s => s.Id == user.StudentId);

            return View(student);
        }

        // GET: Student/RegisterCourse
        public ActionResult RegisterCourse()
        {
            var courses = db.Courses.ToList();
            return View(courses);
        }

        // POST: Student/RegisterCourse/5
        [HttpPost]
        public ActionResult RegisterCourse(int courseId)
        {
            var userId = User.Identity.GetUserId();
            var user = db.Users.Find(userId);

            if (user.StudentId == null)
            {
                return HttpNotFound();
            }

            var enrollment = new Enrollment
            {
                StudentId = user.StudentId.Value,
                CourseId = courseId,
                EnrollmentDate = System.DateTime.Now
            };

            db.Enrollments.Add(enrollment);
            db.SaveChanges();

            return RedirectToAction("MyCourses");
        }
    }
}