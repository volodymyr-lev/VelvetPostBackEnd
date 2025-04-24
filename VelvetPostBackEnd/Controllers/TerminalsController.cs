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
public class TerminalsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public TerminalsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles=("Admin,TerminalEmployee"))]
    public async Task<IActionResult> GetTerminals()
    {
        try
        { 
            var terminals = await _context.Terminals.ToListAsync();
            return Ok(terminals);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при отриманні терміналів {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateTerminal(int id, [FromBody] TerminalDTO updatedTerminal)
    {

        var terminal = await _context.Terminals.FindAsync(id);
        if (terminal == null)
        {
            return NotFound("Термінал не знайдено");
        }

        terminal.Name = updatedTerminal.Name;
        terminal.Address = updatedTerminal.Address;
        terminal.City = updatedTerminal.City;
        terminal.Type = updatedTerminal.Type;

        try
        {
            await _context.SaveChangesAsync();
            return Ok("Термінал успішно оновлено");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при оновленні терміналу {ex.Message}");
        }
    }


    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteTerminal(int id)
    {
        var terminal = await _context.Terminals.FindAsync(id);
        if (terminal == null)
        {
            return NotFound("Термінал не знайдено");
        }

        try
        {
            _context.Terminals.Remove(terminal);
            await _context.SaveChangesAsync();
            return Ok("Термінал успішно видалено");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при видаленні терміналу {ex.Message}");
        }
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateTerminal([FromBody] TerminalDTO terminalDTO)
    {
        if (ModelState.IsValid)
        {
            var terminal = new Terminal
            {
                Name = terminalDTO.Name,
                Address = terminalDTO.Address,
                City = terminalDTO.City,
                Type = terminalDTO.Type
            };

            _context.Terminals.Add(terminal);
            await _context.SaveChangesAsync();
            return Ok("Термінал успішно створено");
        }
        return BadRequest("Невірні дані терміналу");
    }

}


