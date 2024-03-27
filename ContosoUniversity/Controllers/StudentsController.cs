using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using System.Data;

namespace ContosoUniversity.Controllers
{
    public class StudentsController : Controller
    {
        private readonly SchoolContext _context;
        private object fromDate;
        private object toDate;

        public StudentsController(SchoolContext context)
        {
            _context = context;
        }

        // GET: Students
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, DateTime? fromDate, DateTime? toDate, int? pageNumber)
        {
            ViewData["CurrentSort"] = sortOrder;
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            ViewData["CurrentFilter"] = searchString;

            if (searchString != null || fromDate != null || toDate != null)
            {
                pageNumber = 1;
            }
            else
            {
                searchString = currentFilter;
            }
            // ensure page number is at least 1
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            var students = from s in _context.Students
                           select s;
            if (!String.IsNullOrEmpty(searchString))
            {
                students = students.Where(s => s.LastName.Contains(searchString)
                                       || s.FirstMidName.Contains(searchString));
            }
            if (fromDate != null)
            {
                students = students.Where(s => s.EnrollmentDate >= fromDate);
            }

            if (toDate != null)
            {
                students = students.Where(s => s.EnrollmentDate <= toDate);
            }
            switch (sortOrder)
            {
                case "name_desc":
                    students = students.OrderByDescending(s => s.LastName);
                    break;
                case "Date":
                    students = students.OrderBy(s => s.EnrollmentDate);
                    break;
                case "date_desc":
                    students = students.OrderByDescending(s => s.EnrollmentDate);
                    break;
                default:
                    students = students.OrderBy(s => s.LastName);
                    break;
            }
            int pageSize = 3;
            return View(await PaginatedList<Student>.CreateAsync(students.AsNoTracking(), pageNumber ?? 1, pageSize));


          
        }

        // Sync
        public IActionResult AllStudents()
        {
            return View(_context.Students.ToList());
        }

        // GET: Students/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // GET: Students/Create
        public IActionResult Create()
        {
            // Initialize Enrollments property
            var student = new Student
            {
                Enrollments = new List<Enrollment>()
            };

            // Populate dropdown list
            PopulateCoursesDropDownList();

            return View(student);
        }

        // POST: Students/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        // POST: Students/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int enrollmentID, [Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(student);
                    await _context.SaveChangesAsync();

                    // Associate the student with the selected enrollment
                    if (enrollmentID != 0)
                    {
                        var enrollment = await _context.Enrollments.FindAsync(enrollmentID);
                        if (enrollment != null)
                        {
                            enrollment.StudentID = student.ID;
                            _context.Update(enrollment);
                            await _context.SaveChangesAsync();
                        }
                    }

                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the student.");
            }

            // If the model state is not valid or an error occurred, return the view with the student model
            PopulateCoursesDropDownList();
            return View(student);
        }



        private void PopulateCoursesDropDownList()
        {
            // Fetch the list of courses from the database
            var courses = _context.Courses.OrderBy(c => c.Title).ToList();

            // Convert the list of courses to a list of SelectListItem
            var coursesList = courses.Select(c => new SelectListItem
            {
                Value = c.CourseID.ToString(), // Set the value of each item to the CourseID
                Text = c.Title // Set the display text of each item to the Title of the course
            }).ToList();

            // Add a default option to the beginning of the list
            coursesList.Insert(0, new SelectListItem
            {
                Value = "", // Set the value to an empty string
                Text = "-- Select Course --" // Set the display text to indicate selecting a course
            });

            // Set the ViewBag property to the list of courses
            ViewBag.Enrollments = coursesList; // Change ViewBag.Courses to ViewBag.Enrollments
        }

        //create a method to populate the student's available courses when craeting a new student
        private void PopulateAssignedCourseData(Student student)
        {
            var allCourses = _context.Courses;
            var studentCourses = new HashSet<int>(student.Enrollments.Select(c => c.CourseID));
            var viewModel = new List<Enrollment>();
            foreach (var course in allCourses)
            {
                viewModel.Add(new Enrollment
                {
                    CourseID = course.CourseID,
                });
            }
            ViewData["Courses"] = viewModel;
        }
        // GET: Students/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }
            return View(student);
        }

        // POST: Students/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstMidName,EnrollmentDate")] Student student)
        //{
        //    if (id != student.ID)
        //    {
        //        return NotFound();
        //    }

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {
        //            _context.Update(student);
        //            await _context.SaveChangesAsync();
        //        }
        //        catch (DbUpdateConcurrencyException)
        //        {
        //            if (!StudentExists(student.ID))
        //            {
        //                return NotFound();
        //            }
        //            else
        //            {
        //                throw;
        //            }
        //        }
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(student);
        //}

        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var studentToUpdate = await _context.Students.FirstOrDefaultAsync(s => s.ID == id);
            if (await TryUpdateModelAsync<Student>(studentToUpdate,"",s => s.FirstMidName, s => s.LastName, s => s.EnrollmentDate))
            {
                try
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException /* ex */)
                {
                    //Log the error (uncomment ex variable name and write a log.)
                    ModelState.AddModelError("", "Unable to save changes. " +
                        "Try again, and if the problem persists, " +
                        "see your system administrator.");
                }
            }
            return View(studentToUpdate);
        }

        // GET: Students/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var student = await _context.Students
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.ID == id);
            if (student == null)
            {
                return NotFound();
            }

            return View(student);
        }

        // POST: Students/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student != null)
            {
                _context.Students.Remove(student);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StudentExists(int id)
        {
            return _context.Students.Any(e => e.ID == id);
        }
    }
}
