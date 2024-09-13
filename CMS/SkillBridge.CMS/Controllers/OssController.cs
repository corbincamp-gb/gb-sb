using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Dynamic.Core;
using SkillBridge.Business.Data;

namespace SkillBridge.CMS.Controllers
{
    //[Authorize(Roles = "Admin, Analyst, Service")]
    [Route("api/[controller]")]
    [ApiController]
    public class OssController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public OssController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet("GetTest")]
        public IActionResult GetTest()
        {
            return Ok("yes");
        }

        [HttpPost("GetOrgsData")]
        public IActionResult GetOrgsData()
        {
            Console.WriteLine("GetOrgsData Called");
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var orgData = (from temporg in _db.Organizations select temporg);
                Console.WriteLine("orgData created");
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    if(sortColumnDirection == "asc") { sortColumnDirection = "ascending"; }
                    if (sortColumnDirection == "desc") { sortColumnDirection = "descending"; }
                    orgData = orgData.OrderBy(sortColumn + "  " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    /*
                     * <th>Actions</th>
            <th>Active</th>
            <th>Name</th>
            <th>Organization Type</th>
            <th>Date Created</th>
            <th>POC First Name</th>
            <th>POC Last Name</th>
            <th>POC Email</th>
            <th>POC Phone</th>
                    */
                    orgData = orgData.Where(m => m.Name.Contains(searchValue)
                                                || m.Poc_First_Name.Contains(searchValue)
                                                || m.Poc_Last_Name.Contains(searchValue)
                                                || m.Poc_Email.Contains(searchValue)
                                                || m.Poc_Phone.Contains(searchValue));
                }

                /*foreach(var o in oppData)
                {
                    o.Date_Program_Initiated = DateTime.ParseExact(o.Date_Program_Initiated.ToString("MM/dd/yyyy"), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    o.Mou_Expiration_Date = DateTime.ParseExact(o.Mou_Expiration_Date.ToString("MM/dd/yyyy"), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    Console.WriteLine("o.Date_Program_Initiated: " + o.Date_Program_Initiated);
                    Console.WriteLine("o.Mou_Expiration_Date: " + o.Mou_Expiration_Date);
                }*/

                recordsTotal = orgData.Count();
                var data = orgData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };
                
                return Ok(jsonData);


                /*int totalRecord = 0;
                int filterRecord = 0;
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
                var data = _context.Set<Employees>().AsQueryable();
                //get total count of data in table
                totalRecord = data.Count();
                // search data when search value found
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x => x.EmployeeFirstName.ToLower().Contains(searchValue.ToLower()) || x.EmployeeLastName.ToLower().Contains(searchValue.ToLower()) || x.Designation.ToLower().Contains(searchValue.ToLower()) || x.Salary.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                // get total count of records after search
                filterRecord = data.Count();
                //sort data
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection)) data = data.OrderBy(sortColumn + " " + sortColumnDirection);
                //pagination
                var empList = data.Skip(skip).Take(pageSize).ToList();
                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = empList
                };*/

            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOrgsData exception: " + ex.Message);
                throw;
            }
        }

        [HttpPost("GetProgsData")]
        public IActionResult GetProgsData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var progData = (from tempprog in _db.Programs select tempprog);
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    if (sortColumnDirection == "asc") { sortColumnDirection = "ascending"; }
                    if (sortColumnDirection == "desc") { sortColumnDirection = "descending"; }
                    progData = progData.OrderBy(sortColumn + "  " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    progData = progData.Where(m => m.Program_Name.Contains(searchValue)
                                                || m.Organization_Name.Contains(searchValue)
                                                || m.Admin_Poc_First_Name.Contains(searchValue)
                                                || m.Admin_Poc_Last_Name.Contains(searchValue)
                                                || m.Admin_Poc_Email.Contains(searchValue)
                                                || m.Admin_Poc_Phone.Contains(searchValue)
                                                || m.Delivery_Method.Contains(searchValue)
                                                || m.Job_Family.Contains(searchValue)
                                                || m.Services_Supported.Contains(searchValue)
                                                || m.Public_Poc_Name.Contains(searchValue)
                                                || m.Public_Poc_Email.Contains(searchValue));
                }

                /*foreach(var o in oppData)
                {
                    o.Date_Program_Initiated = DateTime.ParseExact(o.Date_Program_Initiated.ToString("MM/dd/yyyy"), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    o.Mou_Expiration_Date = DateTime.ParseExact(o.Mou_Expiration_Date.ToString("MM/dd/yyyy"), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    Console.WriteLine("o.Date_Program_Initiated: " + o.Date_Program_Initiated);
                    Console.WriteLine("o.Mou_Expiration_Date: " + o.Mou_Expiration_Date);
                }*/

                recordsTotal = progData.Count();
                var data = progData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };

                return Ok(jsonData);


                /*int totalRecord = 0;
                int filterRecord = 0;
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
                var data = _context.Set<Employees>().AsQueryable();
                //get total count of data in table
                totalRecord = data.Count();
                // search data when search value found
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x => x.EmployeeFirstName.ToLower().Contains(searchValue.ToLower()) || x.EmployeeLastName.ToLower().Contains(searchValue.ToLower()) || x.Designation.ToLower().Contains(searchValue.ToLower()) || x.Salary.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                // get total count of records after search
                filterRecord = data.Count();
                //sort data
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection)) data = data.OrderBy(sortColumn + " " + sortColumnDirection);
                //pagination
                var empList = data.Skip(skip).Take(pageSize).ToList();
                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = empList
                };*/

            }
            catch (Exception ex)
            {
                Console.WriteLine("GetProgsData exception: " + ex.Message);
                throw;
            }
        }

        [HttpPost("GetOppsData")]
        public IActionResult GetOppsData()
        {
            try
            {
                var draw = Request.Form["draw"].FirstOrDefault();
                var start = Request.Form["start"].FirstOrDefault();
                var length = Request.Form["length"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = length != null ? Convert.ToInt32(length) : 0;
                int skip = start != null ? Convert.ToInt32(start) : 0;
                int recordsTotal = 0;
                var oppData = (from tempopp in _db.Opportunities select tempopp);
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection))
                {
                    if (sortColumnDirection == "asc") { sortColumnDirection = "ascending"; }
                    if (sortColumnDirection == "desc") { sortColumnDirection = "descending"; }
                    oppData = oppData.OrderBy(sortColumn + "  " + sortColumnDirection);
                }
                if (!string.IsNullOrEmpty(searchValue))
                {
                    oppData = oppData.Where(m => m.Program_Name.Contains(searchValue)
                                                || m.Organization_Name.Contains(searchValue)
                                                || m.City.Contains(searchValue)
                                                || m.State.Contains(searchValue)
                                                || m.Employer_Poc_Name.Contains(searchValue)
                                                || m.Employer_Poc_Email.Contains(searchValue));
                }

                /*foreach(var o in oppData)
                {
                    o.Date_Program_Initiated = DateTime.ParseExact(o.Date_Program_Initiated.ToString("MM/dd/yyyy"), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    o.Mou_Expiration_Date = DateTime.ParseExact(o.Mou_Expiration_Date.ToString("MM/dd/yyyy"), "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);

                    Console.WriteLine("o.Date_Program_Initiated: " + o.Date_Program_Initiated);
                    Console.WriteLine("o.Mou_Expiration_Date: " + o.Mou_Expiration_Date);
                }*/

                recordsTotal = oppData.Count();
                var data = oppData.Skip(skip).Take(pageSize).ToList();
                var jsonData = new { draw = draw, recordsFiltered = recordsTotal, recordsTotal = recordsTotal, data = data };

                return Ok(jsonData);


                /*int totalRecord = 0;
                int filterRecord = 0;
                var draw = Request.Form["draw"].FirstOrDefault();
                var sortColumn = Request.Form["columns[" + Request.Form["order[0][column]"].FirstOrDefault() + "][name]"].FirstOrDefault();
                var sortColumnDirection = Request.Form["order[0][dir]"].FirstOrDefault();
                var searchValue = Request.Form["search[value]"].FirstOrDefault();
                int pageSize = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "0");
                int skip = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
                var data = _context.Set<Employees>().AsQueryable();
                //get total count of data in table
                totalRecord = data.Count();
                // search data when search value found
                if (!string.IsNullOrEmpty(searchValue))
                {
                    data = data.Where(x => x.EmployeeFirstName.ToLower().Contains(searchValue.ToLower()) || x.EmployeeLastName.ToLower().Contains(searchValue.ToLower()) || x.Designation.ToLower().Contains(searchValue.ToLower()) || x.Salary.ToString().ToLower().Contains(searchValue.ToLower()));
                }
                // get total count of records after search
                filterRecord = data.Count();
                //sort data
                if (!string.IsNullOrEmpty(sortColumn) && !string.IsNullOrEmpty(sortColumnDirection)) data = data.OrderBy(sortColumn + " " + sortColumnDirection);
                //pagination
                var empList = data.Skip(skip).Take(pageSize).ToList();
                var returnObj = new
                {
                    draw = draw,
                    recordsTotal = totalRecord,
                    recordsFiltered = filterRecord,
                    data = empList
                };*/

            }
            catch (Exception ex)
            {
                Console.WriteLine("GetOppsData exception: " + ex.Message);
                throw;
            }
        }
    }
}