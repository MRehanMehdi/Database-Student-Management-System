using StudentManagementSystem.Models;
using System;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // ──────────────────────────────────────────────
        // DASHBOARD
        // ──────────────────────────────────────────────
        public ActionResult Index()
        {
            ViewBag.TotalStudents = db.Students.Count();
            ViewBag.TotalCourses = db.Courses.Count();
            ViewBag.TotalEnrollments = db.Enrollments.Count();
            ViewBag.TotalAnnouncements = db.Announcements.Count();
            ViewBag.RecentStudents = db.Students.OrderByDescending(s => s.Id).Take(5).ToList();
            ViewBag.RecentAnnouncements = db.Announcements.OrderByDescending(a => a.CreatedAt).Take(3).ToList();
            return View();
        }

        // ──────────────────────────────────────────────
        // STUDENTS
        // ──────────────────────────────────────────────
        public ActionResult Students(string searchString)
        {
            var students = db.Students.AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                string q = searchString.ToLower();
                students = students.Where(s =>
                    s.FirstName.ToLower().Contains(q) ||
                    s.LastName.ToLower().Contains(q) ||
                    s.Email.ToLower().Contains(q));
            }
            return View(students.OrderByDescending(s => s.Id).ToList());
        }

        public ActionResult CreateStudent() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateStudent(Student student)
        {
            if (!ModelState.IsValid) return View(student);

            string photosDir = Server.MapPath("~/Photos/Students/");
            if (!Directory.Exists(photosDir)) Directory.CreateDirectory(photosDir);

            db.Students.Add(student);
            db.SaveChanges();

            if (student.PhotoFile != null && student.PhotoFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(student.PhotoFile.FileName);
                string name = Path.GetFileNameWithoutExtension(student.PhotoFile.FileName) + "_" + student.Id + ext;
                student.PhotoFile.SaveAs(Path.Combine(photosDir, name));
                student.PhotoPath = "~/Photos/Students/" + name;
                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
            }
            return RedirectToAction("Students");
        }

        public ActionResult EditStudent(int id)
        {
            var s = db.Students.Find(id);
            if (s == null) return HttpNotFound();
            return View(s);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditStudent(Student student)
        {
            if (!ModelState.IsValid) return View(student);

            if (student.PhotoFile != null && student.PhotoFile.ContentLength > 0)
            {
                string dir = Server.MapPath("~/Photos/Students/");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                string name = Path.GetFileNameWithoutExtension(student.PhotoFile.FileName) + "_" + student.Id + Path.GetExtension(student.PhotoFile.FileName);
                student.PhotoFile.SaveAs(Path.Combine(dir, name));
                student.PhotoPath = "~/Photos/Students/" + name;
            }

            db.Entry(student).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Students");
        }

        public ActionResult DeleteStudent(int id)
        {
            var s = db.Students.Find(id);
            if (s == null) return HttpNotFound();
            return View(s);
        }

        [HttpPost, ActionName("DeleteStudent"), ValidateAntiForgeryToken]
        public ActionResult DeleteStudentConfirmed(int id)
        {
            var s = db.Students.Find(id);
            if (s != null) { db.Students.Remove(s); db.SaveChanges(); }
            return RedirectToAction("Students");
        }

        public ActionResult DetailStudent(int? id)
        {
            if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var s = db.Students.Include(x => x.Enrollments.Select(e => e.Course)).FirstOrDefault(x => x.Id == id);
            if (s == null) return HttpNotFound();
            return View(s);
        }

        public ActionResult StudentCourses(int id)
        {
            var s = db.Students.Include(x => x.Enrollments.Select(e => e.Course)).FirstOrDefault(x => x.Id == id);
            if (s == null) return HttpNotFound();
            return View(s);
        }

        // ──────────────────────────────────────────────
        // COURSES
        // ──────────────────────────────────────────────
        public ActionResult Courses()
        {
            var courses = db.Courses.Include(c => c.Enrollments).ToList();
            return View(courses);
        }

        public ActionResult CreateCourse() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateCourse(Course course)
        {
            if (!ModelState.IsValid) return View(course);
            db.Courses.Add(course);
            db.SaveChanges();
            return RedirectToAction("Courses");
        }

        public ActionResult EditCourse(int id)
        {
            var c = db.Courses.Find(id);
            if (c == null) return HttpNotFound();
            return View(c);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditCourse(Course course)
        {
            if (!ModelState.IsValid) return View(course);
            db.Entry(course).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Courses");
        }

        public ActionResult DeleteCourse(int id)
        {
            var c = db.Courses.Find(id);
            if (c == null) return HttpNotFound();
            db.Courses.Remove(c);
            db.SaveChanges();
            return RedirectToAction("Courses");
        }

        // ──────────────────────────────────────────────
        // ENROLLMENTS
        // ──────────────────────────────────────────────
        public ActionResult Enrollments()
        {
            var enrollments = db.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrollmentDate)
                .ToList();
            return View(enrollments);
        }

        // ──────────────────────────────────────────────
        // GRADES
        // ──────────────────────────────────────────────
        public ActionResult Grades()
        {
            ViewBag.Grades = db.Grades
                .Include(g => g.Enrollment.Student)
                .Include(g => g.Enrollment.Course)
                .OrderByDescending(g => g.DateAssigned)
                .ToList();
            return View();
        }

        public ActionResult AssignGrade()
        {
            ViewBag.Enrollments = db.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Select(e => new {
                    e.Id,
                    Display = e.Student.FirstName + " " + e.Student.LastName + " — " + e.Course.CourseName
                })
                .ToList()
                .Select(e => new SelectListItem { Value = e.Id.ToString(), Text = e.Display });
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult AssignGrade(Grade grade)
        {
            if (!ModelState.IsValid) return View(grade);
            grade.DateAssigned = DateTime.Now;

            // Auto-calculate letter grade
            grade.LetterGrade = grade.Score >= 90 ? "A"
                : grade.Score >= 80 ? "B"
                : grade.Score >= 70 ? "C"
                : grade.Score >= 60 ? "D" : "F";

            db.Grades.Add(grade);
            db.SaveChanges();
            return RedirectToAction("Grades");
        }

        public ActionResult EditGrade(int id)
        {
            var g = db.Grades.Find(id);
            if (g == null) return HttpNotFound();
            return View(g);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult EditGrade(Grade grade)
        {
            if (!ModelState.IsValid) return View(grade);
            grade.LetterGrade = grade.Score >= 90 ? "A"
                : grade.Score >= 80 ? "B"
                : grade.Score >= 70 ? "C"
                : grade.Score >= 60 ? "D" : "F";
            db.Entry(grade).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Grades");
        }

        // ──────────────────────────────────────────────
        // ANNOUNCEMENTS
        // ──────────────────────────────────────────────
        public ActionResult Announcements()
        {
            ViewBag.Announcements = db.Announcements.OrderByDescending(a => a.CreatedAt).ToList();
            return View();
        }

        public ActionResult CreateAnnouncement() => View();

        [HttpPost, ValidateAntiForgeryToken]
        public ActionResult CreateAnnouncement(Announcement ann)
        {
            if (!ModelState.IsValid) return View(ann);
            ann.CreatedAt = DateTime.Now;
            db.Announcements.Add(ann);
            db.SaveChanges();
            return RedirectToAction("Announcements");
        }

        public ActionResult DeleteAnnouncement(int id)
        {
            var a = db.Announcements.Find(id);
            if (a != null) { db.Announcements.Remove(a); db.SaveChanges(); }
            return RedirectToAction("Announcements");
        }

        // ──────────────────────────────────────────────
        // REPORTS
        // ──────────────────────────────────────────────
        public ActionResult StudentReport()
        {
            return View(db.Students.ToList());
        }

        public ActionResult GenerateStudentReport()
        {
            var students = db.Students.ToList();
            var sb = new StringBuilder();
            sb.AppendLine("Id,FirstName,LastName,Email,PhoneNumber,DateOfBirth,Address");
            foreach (var s in students)
                sb.AppendLine($"{s.Id},{Esc(s.FirstName)},{Esc(s.LastName)},{Esc(s.Email)},{Esc(s.PhoneNumber)},{s.DateOfBirth:yyyy-MM-dd},{Esc(s.Address)}");
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "StudentReport.csv");
        }

        public ActionResult AdminProfile() => View();

        private string Esc(string f)
        {
            if (string.IsNullOrEmpty(f)) return "";
            if (f.Contains(",") || f.Contains("\"") || f.Contains("\n"))
                return $"\"{f.Replace("\"", "\"\"")}\"";
            return f;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
