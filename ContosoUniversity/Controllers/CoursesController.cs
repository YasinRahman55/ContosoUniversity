﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.SchoolViewModels;

namespace ContosoUniversity.Controllers
{
    public class CoursesController : Controller
    {
        private readonly SchoolContext _context;

        public CoursesController(SchoolContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(int? departmentId, int? id, int? courseID)
        {
            var viewModel = new InstructorIndexData();

            // Query courses
            IQueryable<Course> coursesQuery = _context.Courses
                .Include(i => i.Department)
                .Include(i => i.Enrollments)
                    .ThenInclude(i => i.Student)
                .AsNoTracking()
                .OrderBy(i => i.Title);

            // Filter by department if departmentId is provided
            if (departmentId != null)
            {
                coursesQuery = coursesQuery.Where(c => c.DepartmentID == departmentId.Value);
            }

            // Execute the query and assign the result to viewModel.Courses
            viewModel.Courses = await coursesQuery.ToListAsync();

            // If courseID is provided, filter the viewModel.Courses and set viewModel.Instructors
            if (courseID != null)
            {
                ViewData["CourseId"] = courseID.Value;

                Course course = viewModel.Courses.Where(i => i.CourseID == courseID.Value).Single();
                if (course != null)
                {
                    viewModel.Instructors = course.CourseAssignments.Select(ca => ca.Instructor);
                }
            }

            // If id is provided, filter viewModel.Instructors and set viewModel.CourseAssignments
            if (id.HasValue && viewModel.Instructors != null)
            {
                Instructor instructor = viewModel.Instructors.FirstOrDefault(i => i.ID == id);
                if (instructor != null)
                {
                    viewModel.CourseAssignments = instructor.CourseAssignments;
                }
            }

            return View(viewModel);
        }

        //public async Task<IActionResult> Index(int? departmentId, int? id, int? courseID)
        //{
        //    var viewModel = new InstructorIndexData();
        //    viewModel.Courses = await _context.Courses
        //        .Include(i => i.Department)
        //        .Include(i => i.Enrollments)
        //            .ThenInclude(i => i.Student)
        //        .AsNoTracking()
        //        .OrderBy(i => i.Title)
        //        .ToListAsync(); 
        //    //viewModel.Instructors = await _context.Instructors
        //    //      .Include(i => i.OfficeAssignment)
        //    //      .Include(i => i.CourseAssignments)
        //    //        .ThenInclude(i => i.Course)
        //    //            .ThenInclude(i => i.Enrollments)
        //    //                .ThenInclude(i => i.Student)
        //    //      .Include(i => i.CourseAssignments)
        //    //        .ThenInclude(i => i.Course)
        //    //            .ThenInclude(i => i.Department)
        //    //      .AsNoTracking()
        //    //      .OrderBy(i => i.LastName)
        //    //      .ToListAsync();

        //    if (courseID != null)
        //    {
        //        ViewData["CourseId"] = courseID.Value;
        //        Course course = viewModel.Courses.Where(i => i.CourseID == courseID.Value).Single();
        //        viewModel.Instructors = course.CourseAssignments.Select(s => s.Instructor);
        //    }

        //    if (id != null)
        //    {
        //        viewModel.CourseAssignments = viewModel.Instructors.Where(x => x.ID == id).Single().CourseAssignments;

        //    }
        //    return View(viewModel);
        //}

        // GET: Courses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            PopulateDepartmentsDropDownList();
            ////ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name");
            return View();
        }

        // POST: Courses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CourseID,Title,Credits,DepartmentID")] Course course)
        {
            if (ModelState.IsValid)
            {
                _context.Add(course);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            /////ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return NotFound();
            }
            PopulateDepartmentsDropDownList(course.DepartmentID);
            ///// ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }

        // POST: Courses/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CourseID,Title,Credits,DepartmentID")] Course course)
        {
            if (id != course.CourseID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(course);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CourseExists(course.CourseID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            PopulateDepartmentsDropDownList(course.DepartmentID);
            ////ViewData["DepartmentID"] = new SelectList(_context.Departments, "DepartmentID", "Name", course.DepartmentID);
            return View(course);
        }
        private void PopulateDepartmentsDropDownList(object selectedDepartment = null)
        {
            var departmentsQuery = from d in _context.Departments
                                   orderby d.Name
                                   select d;
            ViewBag.departmentId = new SelectList(departmentsQuery.AsNoTracking(), "DepartmentID", "Name", selectedDepartment);
        }


        // GET: Courses/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var course = await _context.Courses
                .Include(c => c.Department)
                .FirstOrDefaultAsync(m => m.CourseID == id);
            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // POST: Courses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var course = await _context.Courses.FindAsync(id);
            if (course != null)
            {
                _context.Courses.Remove(course);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CourseExists(int id)
        {
            return _context.Courses.Any(e => e.CourseID == id);
        }
    }
}
