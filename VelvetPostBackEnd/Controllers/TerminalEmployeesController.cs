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
}
