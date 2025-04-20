using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web.Resource;
using VelvetPostBackEnd.Data;
using System.Security.Claims;
using VelvetPostBackEnd.DTOs;
using VelvetPostBackEnd.Models;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Identity.Client.Kerberos;
using System.IdentityModel.Tokens.Jwt;

namespace VelvetPostBackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO regDTO)
    {
        if (ModelState.IsValid)
        {
            var user = new ApplicationUser
            {
                UserName = regDTO.Email,
                Email = regDTO.Email,
                PhoneNumber = regDTO.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, regDTO.Password);

            if (result.Succeeded)
            {
                var client = new Client
                {
                    FirstName = regDTO.FirstName,
                    LastName = regDTO.LastName,
                    Email = regDTO.Email,
                    PhoneNumber = regDTO.PhoneNumber,
                    Address = regDTO.Address,
                    City = regDTO.City,
                    ApplicationUserId = user.Id
                };

                _context.Clients.Add(client);
                await _userManager.AddToRoleAsync(user, "Client");
                return Ok(new { Message = "Client account created successfully." });
            }

            return BadRequest(result.Errors);
        }

        return BadRequest("Invalid registration data");
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        if (ModelState.IsValid)
        {
            var user = await _context.Users
                .Include(u => u.Client)
                .Include(u => u.Employee)
                    .ThenInclude(e => e.PostOfficeEmployee)
                .Include(u => u.Employee)
                    .ThenInclude(e => e.TerminalEmployee)
                .FirstOrDefaultAsync(u=>u.Email == loginDTO.Email);

            if(user != null)
            {
                var result = await _signInManager.PasswordSignInAsync(user, loginDTO.Password, false, false);
                if (result.Succeeded)
                {
                    var token = await GenerateJWTToken(user);

                    var roles = await _userManager.GetRolesAsync(user);
                    var userType = user.Client != null ? "Client" :
                        user.Employee != null ? "Employee" : "Unknown";

                    object profile = null;
                    if(userType == "Client")
                    {
                        profile = new
                        {
                            Id = user.Client.Id,
                            FirstName = user.Client.FirstName,
                            LastName = user.Client.LastName,
                            Email = user.Client.Email,
                            Phone = user.Client.PhoneNumber,
                            Address = user.Client.Address,
                            City = user.Client.City,
                        };
                    }
                    else if(userType == "Employee")
                    {
                        var employeeType = user.Employee.PostOfficeEmployee != null ? "PostOffice" :
                                      user.Employee.TerminalEmployee != null ? "Terminal" : "Unknown";

                        profile = new
                        {
                            Id = user.Employee.Id,
                            FirstName = user.Employee.FirstName,
                            LastName = user.Employee.LastName,
                            Email = user.Employee.Email,
                            Phone = user.Employee.PhoneNumber,
                            Position = user.Employee.Position,
                            StartDate = user.Employee.StartDate,
                            EmployeeType = employeeType,
                            PostOfficeId = user.Employee.PostOfficeEmployee?.PostOfficeId,
                            TerminalId = user.Employee.TerminalEmployee?.TerminalId
                        };
                    }

                    return Ok(new
                    {
                        Token = token,
                        Role = roles.FirstOrDefault(),
                        Type = userType,
                        Profile = profile
                    });
                }
                Console.WriteLine($"Invalid login attempt for email: {loginDTO.Email}");
                return Unauthorized("Invalid login attempt");
            }
            Console.WriteLine($"User not found: {loginDTO.Email}");
            return Unauthorized("User not found");
        }

        return BadRequest("Invalid login data");
    }



    // JWT generation fucntion
    public async Task<string> GenerateJWTToken(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),

        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.Now.AddHours(5),
            signingCredentials: credentials
        );


        Console.WriteLine("Token: ");
        Console.WriteLine(token.EncodedPayload);
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}