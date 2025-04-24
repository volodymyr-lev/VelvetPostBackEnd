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
public class PostOfficeEmployeesController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public PostOfficeEmployeesController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetPostOfficeEmployees()
    {
        try
        {
            var postOfficeEmployees = await _context.PostOfficeEmployees.
                Include(e=>e.Employee)
                .Include(po=>po.PostOffice)
                .ToListAsync();

            var result = postOfficeEmployees.Select(poe => new
            {
                Id = poe.Id,
                EmployeeId = poe.Employee.Id,
                PostOfficeId = poe.PostOffice.Id,
                Employee = new EmployeeDTO
                {
                    Id = poe.Employee.Id,
                    FirstName = poe.Employee.FirstName,
                    LastName = poe.Employee.LastName,
                    Position = poe.Employee.Position,
                    PhoneNumber = poe.Employee.PhoneNumber,
                    Email = poe.Employee.Email,
                    StartDate = poe.Employee.StartDate
                },
                PostOffice = new PostOfficeDTO
                {
                    Id = poe.PostOffice.Id,
                    Name = poe.PostOffice.Name,
                    Address = poe.PostOffice.Address,
                    City = poe.PostOffice.City,
                    PhoneNumber = poe.PostOffice.PhoneNumber,
                    TerminalId = poe.PostOffice.TerminalId.GetValueOrDefault()
                }
            }).ToList();


            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при отриманні співробітників відділення {ex.Message}");
        }
    }
}
