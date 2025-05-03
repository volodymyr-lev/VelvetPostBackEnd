using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VelvetPostBackEnd.Data;
using VelvetPostBackEnd.Models;
using VelvetPostBackEnd.DTOs;

namespace VelvetPostBackEnd.Controllers;


[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public ClientsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetClient(int id)
    {
        var client = await _context.Clients.Include(c => c.ApplicationUser)
                                           .FirstOrDefaultAsync(c => c.Id == id);
        if (client == null)
        {
            return NotFound("Client with this Id was not found");
        }

        var shipments = await _context.Shipments
                        .Where(s => s.ReceiverId == id || s.SenderId == id)
                        .ToListAsync();


        return Ok(new GetClientDTO
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            City = client.City,
            Address = client.Address,
            CreationDate = client.ApplicationUser.CreatedAt,
            Shipments = shipments.Select(s => new ClientShipmentDTO
            {
                Id = s.Id,
                SenderId = s.SenderId,
                ReceiverId = s.ReceiverId,
                FromPostOfficeId = s.FromPostOfficeId,
                ToPostOfficeId = s.ToPostOfficeId,
                CreatedAt = s.CreatedAt,
                DeliveredAt = s.DeliveredAt,
                Status = s.Status,
                Price = s.Price,
                ParcelId = s.ParcelId,
            }).ToList()
        });
    }

    [HttpGet("by-email/{email}")]
    public async Task<IActionResult> GetClientByEmail(string email)
    {
        var client = await _context.Clients
            .Include(c => c.ApplicationUser)
            .FirstOrDefaultAsync(c => c.Email == email);

        if (client == null)
        {
            return NotFound("Client with this email was not found");
        }

        var shipments = await _context.Shipments
            .Where(s => s.ReceiverId == client.Id || s.SenderId == client.Id)
            .ToListAsync();

        return Ok(new GetClientDTO
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            City = client.City,
            Address = client.Address,
            CreationDate = client.ApplicationUser.CreatedAt,
            Shipments = shipments.Select(s => new ClientShipmentDTO
            {
                Id = s.Id,
                SenderId = s.SenderId,
                ReceiverId = s.ReceiverId,
                FromPostOfficeId = s.FromPostOfficeId,
                ToPostOfficeId = s.ToPostOfficeId,
                CreatedAt = s.CreatedAt,
                DeliveredAt = s.DeliveredAt,
                Status = s.Status,
                Price = s.Price,
            }).ToList()
        });
    }

    [HttpGet("by-phone/{phoneNumber}")]
    public async Task<IActionResult> GetClientByPhone(string phoneNumber)
    {
        var client = await _context.Clients
            .Include(c => c.ApplicationUser)
            .FirstOrDefaultAsync(c => c.PhoneNumber == phoneNumber);
        if (client == null)
        {
            return NotFound("Client with this phone number was not found");
        }
        var shipments = await _context.Shipments
            .Where(s => s.ReceiverId == client.Id || s.SenderId == client.Id)
            .ToListAsync();
        return Ok(new GetClientDTO
        {
            Id = client.Id,
            FirstName = client.FirstName,
            LastName = client.LastName,
            Email = client.Email,
            PhoneNumber = client.PhoneNumber,
            City = client.City,
            Address = client.Address,
            CreationDate = client.ApplicationUser.CreatedAt,
            Shipments = shipments.Select(s => new ClientShipmentDTO
            {
                Id = s.Id,
                SenderId = s.SenderId,
                ReceiverId = s.ReceiverId,
                FromPostOfficeId = s.FromPostOfficeId,
                ToPostOfficeId = s.ToPostOfficeId,
                CreatedAt = s.CreatedAt,
                DeliveredAt = s.DeliveredAt,
                Status = s.Status,
                Price = s.Price,
            }).ToList()
        });
    }

    [HttpPost("client-create-shipment")]
    public async Task<IActionResult> ClientCreateShipment([FromBody] ClientCreateShipmentDTO clientCreateShipmentDTO)
    {
        try
        {
            var newShipment = new Shipment
            {
                SenderId = clientCreateShipmentDTO.SenderId,
                ReceiverId = clientCreateShipmentDTO.ReceiverId,
                FromPostOfficeId = clientCreateShipmentDTO.FromPostOfficeId,
                ToPostOfficeId = clientCreateShipmentDTO.ToPostOfficeId,
                Price = 75.00,
                CreatedAt = DateTime.UtcNow,
                Status = "Очікує пакунок",
                ParcelId = null,
            };

            var result = await _context.Shipments.AddAsync(newShipment);
            await _context.SaveChangesAsync();
            return Ok($"Відправлення {result.Entity.Id} створено.");

        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}