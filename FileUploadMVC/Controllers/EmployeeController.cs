using FileUploadMVC.Context;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace FileUploadMVC.Controllers
{
    public class EmployeeController : Controller
    {
        FileUploadMVCEntities _context = new FileUploadMVCEntities();
        
        // GET: Employee
        public ActionResult Index()
        {
            var employeeList = _context.Employees.ToList();
            return View(employeeList);
        }

        //Create Employee
        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create(Employee employee, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                // Check if employee already exists
                var existEmployee = _context.Employees.Any(e => e.EmployeeEmail == employee.EmployeeEmail);

                if (existEmployee)
                {
                    ModelState.AddModelError("EmployeeEmail", "Employee already registered");
                    return View(employee);
                }

                if (file != null && file.ContentLength > 0)
                {
                    // Generate a unique filename
                    var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                    var extension = Path.GetExtension(file.FileName);
                    var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{extension}";

                    // Define the path to save the file
                    var path = Path.Combine(Server.MapPath("~/Uploads"), uniqueFileName);
                    file.SaveAs(path);

                    employee.Profile = $"~/Uploads/{uniqueFileName}";
                }

                employee.LastUpdate = DateTime.Now;

                _context.Employees.Add(employee);
                _context.SaveChanges();

                ModelState.Clear();
                return RedirectToAction("Index");
            }

            return View(employee);
        }

        //Employee Edit
        public ActionResult Edit(int employeeId)
        {
            var existEmployee = _context.Employees.Find(employeeId);
            if (existEmployee == null)
            {
                return HttpNotFound();
            }

            return View(existEmployee);
        }

        [HttpPost]
        public ActionResult Edit(Employee employee, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                var existingEmployee = _context.Employees.Find(employee.EmployeeId);
                if (existingEmployee != null)
                {
                    // Delete the old profile image if a new one is uploaded
                    if (file != null && file.ContentLength > 0)
                    {
                        // Delete the old file
                        if (!string.IsNullOrEmpty(existingEmployee.Profile))
                        {
                            var oldFilePath = Server.MapPath(existingEmployee.Profile);
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Save the new file
                        var fileName = Path.GetFileNameWithoutExtension(file.FileName);
                        var extension = Path.GetExtension(file.FileName);
                        var uniqueFileName = $"{fileName}_{Guid.NewGuid()}{extension}";
                        var path = Path.Combine(Server.MapPath("~/Uploads"), uniqueFileName);
                        file.SaveAs(path);

                        employee.Profile = $"~/Uploads/{uniqueFileName}";
                    }
                    else
                    {
                        // Preserve the existing profile path if no new file is uploaded
                        employee.Profile = existingEmployee.Profile;
                    }

                    existingEmployee.EmployeeName = employee.EmployeeName;
                    existingEmployee.EmployeeEmail = employee.EmployeeEmail;
                    existingEmployee.Password = employee.Password;
                    existingEmployee.Address = employee.Address;
                    existingEmployee.Profile = employee.Profile;
                    existingEmployee.LastUpdate = DateTime.Now;

                    _context.Entry(existingEmployee).State = EntityState.Modified;
                    _context.SaveChanges();

                    return RedirectToAction("Index");
                }
            }

            return View(employee);
        }

        //Delete Employee
        public ActionResult Delete(int employeeId)
        {
            var existEmployee = _context.Employees.Find(employeeId);
            if (existEmployee == null)
            {
                return HttpNotFound();
            }

            // Delete the profile image file if it exists
            if (!string.IsNullOrEmpty(existEmployee.Profile))
            {
                var filePath = Server.MapPath(existEmployee.Profile);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            _context.Employees.Remove(existEmployee);
            _context.SaveChanges();

            var employeeList = _context.Employees.ToList();
            return View("Index", employeeList);
        }


    }
}