using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Mvc;
using StudentManagementSystem.Models;

namespace StudentManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Admin/Index
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Students
        public ActionResult Students(string searchString)
        {
            var students = db.Students.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                string lowerSearch = searchString.ToLower();
                students = students.Where(s => s.FirstName.ToLower().Contains(lowerSearch) || s.LastName.ToLower().Contains(lowerSearch));
            }

            return View(students.ToList());
        }

        // GET: Admin/CreateStudent
        public ActionResult CreateStudent()
        {
            return View();
        }

        // POST: Admin/CreateStudent
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CreateStudent(Student student)
        {
            if (ModelState.IsValid)
            {
                string photosDir = Server.MapPath("~/Photos/Students/");
                if (!Directory.Exists(photosDir))
                {
                    Directory.CreateDirectory(photosDir);
                }

                db.Students.Add(student);
                db.SaveChanges();

                if (student.PhotoFile != null && student.PhotoFile.ContentLength > 0)
                {
                    string fileName = Path.GetFileNameWithoutExtension(student.PhotoFile.FileName);
                    string extension = Path.GetExtension(student.PhotoFile.FileName);
                    string uniqueFileName = fileName + "_" + student.Id + extension;

                    string savePath = Path.Combine(photosDir, uniqueFileName);
                    student.PhotoFile.SaveAs(savePath);

                    student.PhotoPath = "~/Photos/Students/" + uniqueFileName;
                    db.Entry(student).State = EntityState.Modified;
                    db.SaveChanges();
                }

                return RedirectToAction("Students");
            }

            return View(student);
        }

        // GET: Admin/EditStudent/5
        public ActionResult EditStudent(int id)
        {
            var student = db.Students.Find(id);
            if (student == null)
                return HttpNotFound();

            return View(student);
        }

        // POST: Admin/EditStudent/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditStudent(Student student)
        {
            if (ModelState.IsValid)
            {
                if (student.PhotoFile != null && student.PhotoFile.ContentLength > 0)
                {
                    string photosDir = Server.MapPath("~/Photos/Students/");
                    if (!Directory.Exists(photosDir))
                    {
                        Directory.CreateDirectory(photosDir);
                    }

                    string fileName = Path.GetFileNameWithoutExtension(student.PhotoFile.FileName);
                    string extension = Path.GetExtension(student.PhotoFile.FileName);
                    string uniqueFileName = fileName + "_" + student.Id + extension;

                    string savePath = Path.Combine(photosDir, uniqueFileName);
                    student.PhotoFile.SaveAs(savePath);
                    student.PhotoPath = "~/Photos/Students/" + uniqueFileName;
                }

                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Students");
            }
            return View(student);
        }

        // GET: Admin/DeleteStudent/5
        public ActionResult DeleteStudent(int id)
        {
            var student = db.Students.Find(id);
            if (student == null)
                return HttpNotFound();

            return View(student);
        }

        // POST: Admin/DeleteStudent/5
        [HttpPost, ActionName("DeleteStudent")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteStudentConfirmed(int id)
        {
            var student = db.Students.Find(id);
            if (student != null)
            {
                db.Students.Remove(student);
                db.SaveChanges();
            }
            return RedirectToAction("Students");
        }

        // GET: Admin/DetailStudent/5
        public ActionResult DetailStudent(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: Admin/StudentCourses/5
        public ActionResult StudentCourses(int id)
        {
            var student = db.Students.Include(s => s.Enrollments.Select(e => e.Course))
                                     .FirstOrDefault(s => s.Id == id);
            if (student == null)
            {
                return HttpNotFound();
            }
            return View(student);
        }

        // GET: Admin/ProfileAdmin
        public ActionResult AdminProfile()
        {
            return View();
        }

        // ✅ New action: CSV file download
        public ActionResult GenerateStudentReport()
        {
            var students = db.Students.ToList();

            if (students == null || !students.Any())
            {
                TempData["Error"] = "No student data available for the report.";
                return RedirectToAction("Students");
            }

            var sb = new StringBuilder();
            sb.AppendLine("Id,FirstName,LastName,Email,PhoneNumber");

            foreach (var s in students)
            {
                string firstName = EscapeCsvField(s.FirstName);
                string lastName = EscapeCsvField(s.LastName);
                string email = EscapeCsvField(s.Email);
                string phone = EscapeCsvField(s.PhoneNumber);

                sb.AppendLine($"{s.Id},{firstName},{lastName},{email},{phone}");
            }

            byte[] buffer = Encoding.UTF8.GetBytes(sb.ToString());
            return File(buffer, "text/csv", "StudentReport.csv");
        }

        // ✅ New action: View-based Student Report (HTML table)
        public ActionResult StudentReport()
        {
            var students = db.Students.ToList();
            return View(students); // Make sure StudentReport.cshtml exists in Views/Admin
        }

        // Helper for escaping CSV fields
        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";

            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                field = field.Replace("\"", "\"\"");
                return $"\"{field}\"";
            }

            return field;
        }
            
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
