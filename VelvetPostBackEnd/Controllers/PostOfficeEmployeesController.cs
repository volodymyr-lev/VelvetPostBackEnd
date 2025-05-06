using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
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
                Include(e => e.Employee)
                .Include(po => po.PostOffice)
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

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AddPostOfficeEmployee([FromBody] CreateEmpoyeeDTO employee)
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
                var postOfficeEmployee = new PostOfficeEmployee
                {
                    EmployeeId = newEmployee.Id,
                    PostOfficeId = employee.DepId
                };
                await _context.PostOfficeEmployees.AddAsync(postOfficeEmployee);
                await _context.SaveChangesAsync();
                await _userManager.AddToRoleAsync(user, "PostOfficeEmployee");
            }
            else
            {
                return BadRequest("User creation failed");
            }
            
            return Ok("PostOfficeEmployee added successfully");
        }

        return BadRequest("Invalid model state.");
    }

    [HttpPost("CreateShipmentWithParcel")]
    [Authorize(Roles = "PostOfficeEmployee")]
    public async Task<IActionResult> CreateShipmentWithParcel([FromBody] CreateShipmentWithParcelDTO createShipmentWithParcelDTO)
    {
        if (ModelState.IsValid)
        {
            var sender = await _context.Clients.FirstOrDefaultAsync(c => c.PhoneNumber == createShipmentWithParcelDTO.SenderPhone);
            if (sender == null)
            {
                return BadRequest("Відправляча не знайдено");
            }

            var reciver = await _context.Clients.FirstOrDefaultAsync(c=>c.PhoneNumber == createShipmentWithParcelDTO.RecipientPhone);
            if(reciver == null)
            {
                return BadRequest("Отримувача не знайдено");
            }

            if(createShipmentWithParcelDTO.Weight > 500)
            {
                return BadRequest("Вага посилки не може перевищувати 500 кілограмів");
            }

            var parcel = new Parcel
            {
                Type = createShipmentWithParcelDTO.Type,
                Weight = createShipmentWithParcelDTO.Weight
            };

            _context.Parcels.Add(parcel);
            await _context.SaveChangesAsync();

            var shipment = new Shipment
            {
                Price = createShipmentWithParcelDTO.Price,
                SenderId = sender.Id,
                ReceiverId = reciver.Id,
                FromPostOfficeId = createShipmentWithParcelDTO.FromOffice,
                ToPostOfficeId = createShipmentWithParcelDTO.ToOffice,
                ParcelId = parcel.Id,
                CreatedAt = DateTime.UtcNow,
                Status = "Очікує відправки"
            };

            _context.Shipments.Add(shipment);
            await _context.SaveChangesAsync();

            return Ok("Відправлення створено");
        }
        return BadRequest("Invalid model state");
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDTO updateData)
    {
        Console.WriteLine($"name {updateData.FirstName} {updateData.LastName}\n" +
                          $"email {updateData.Email} pn {updateData.PhoneNumber}\n"+
                          $"position {updateData.Position} depId {updateData.DepId}");


        var employee = await _context.Employees
                .Include(e => e.PostOfficeEmployee)
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
            employee.PostOfficeEmployee.PostOfficeId = updateData.DepId;

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
        var employee = await _context.Employees.Include(e => e.PostOfficeEmployee)
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
            _context.PostOfficeEmployees.Remove(employee.PostOfficeEmployee);
            await _userManager.DeleteAsync(employee.ApplicationUser);
            return Ok("Employee deleted successfully");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error occurred while deleting the employee: {ex.Message}");
        }
    }
}
