using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VelvetPostBackEnd.Data;
using VelvetPostBackEnd.DTOs;
using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.Controllers;


[Route("api/[controller]")]
[ApiController]
public class TerminalEmployeesController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public TerminalEmployeesController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }


    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetTerminalEmployees()
    {
        try
        {
            var terminalEmployees = await _context.TerminalEmployees
                .Include(e => e.Employee)
                .Include(t => t.Terminal)
                .ToListAsync();


            var result = terminalEmployees.Select(te => new
            {
                Id = te.Id,
                EmployeeId = te.Employee.Id,
                TerminalId = te.Terminal.Id,
                Employee = new EmployeeDTO
                {
                    Id = te.Employee.Id,
                    FirstName = te.Employee.FirstName,
                    LastName = te.Employee.LastName,
                    Position = te.Employee.Position,
                    PhoneNumber = te.Employee.PhoneNumber,
                    Email = te.Employee.Email,
                    StartDate = te.Employee.StartDate
                },
                Terminal = new TerminalDTO
                {
                    Id = te.Terminal.Id,
                    Name = te.Terminal.Name,
                    Address = te.Terminal.Address,
                    City = te.Terminal.City,
                    Type = te.Terminal.Type
                }
            }).ToList();
            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при отриманні співробітників відділення {ex.Message}");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddTerminalEmployee([FromBody] CreateEmpoyeeDTO employee)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = employee.Email,
                Email = employee.Email,
                PhoneNumber = employee.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, employee.Password);
            if (result.Succeeded)
            {
                var newEmployee = new Employee
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Position = employee.Position,
                    PhoneNumber = employee.PhoneNumber,
                    Email = employee.Email,
                    StartDate = DateTime.UtcNow,
                    ApplicationUserId = user.Id
                };

                await _context.Employees.AddAsync(newEmployee);
                await _context.SaveChangesAsync();
                var terminalEmployee = new TerminalEmployee
                {
                    EmployeeId = newEmployee.Id,
                    TerminalId = employee.DepId
                };
                await _context.TerminalEmployees.AddAsync(terminalEmployee);
                await _context.SaveChangesAsync();
                await _userManager.AddToRoleAsync(user, "TerminalEmployee");
                return Ok("TerminalEmployee added successfully");
            }
            else
            {
                return BadRequest("User creation failed");
            }
        }
        return BadRequest("Invalid model state.");
    }


    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDTO updateData)
    {
        Console.WriteLine($"name {updateData.FirstName} {updateData.LastName}\n" +
                          $"email {updateData.Email} pn {updateData.PhoneNumber}\n" +
                          $"position {updateData.Position} depId {updateData.DepId}");


        var employee = await _context.Employees
                .Include(e => e.TerminalEmployee)
                .Include(e => e.ApplicationUser)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            return NotFound("Couldn't find employee or department");
        }

        try
        {
            // update employee data
            employee.FirstName = updateData.FirstName;
            employee.LastName = updateData.LastName;
            employee.Email = updateData.Email;
            employee.PhoneNumber = updateData.PhoneNumber;
            employee.Position = updateData.Position;
            employee.TerminalEmployee.TerminalId = updateData.DepId;

            // update ApplicationUser data
            employee.ApplicationUser.UserName = updateData.Email;
            employee.ApplicationUser.Email = updateData.Email;
            employee.ApplicationUser.PhoneNumber = updateData.PhoneNumber;

            await _context.SaveChangesAsync();
            return Ok("Employee updated successfully");
        }
        catch (Exception)
        {
            return StatusCode(500, "Concurrency error occurred while updating the employee.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.Include(e => e.TerminalEmployee)
                                               .Include(e => e.ApplicationUser)
                                               .FirstOrDefaultAsync(e => e.Id == id);

        if (employee == null)
        {
            return NotFound("Couldn't find employee or department");
        }

        try
        {
            // remove employye from employees, postoffice employees and AppUser
            _context.Employees.Remove(employee);
            _context.TerminalEmployees.Remove(employee.TerminalEmployee);
            await _userManager.DeleteAsync(employee.ApplicationUser);
            return Ok("Employee deleted successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error occurred while deleting the employee: {ex.Message}");
        }
    }
}
