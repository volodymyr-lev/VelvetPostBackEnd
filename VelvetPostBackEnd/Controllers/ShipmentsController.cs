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
public class ShipmentsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public ShipmentsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetShipmentById(int id)
    {
        try
        {
            var shipment = await _context.Shipments
                                         .Include(s => s.Parcel)
                                         .Include(s=>s.Sender)
                                         .Include(s=>s.Receiver)
                                         .Include(s=>s.FromPostOffice)
                                         .Include(s=> s.ToPostOffice)
                                         .FirstAsync(s => s.Id == id);

            if(shipment == null)
            {
                return NotFound($"No shipment with id = {id}.");
            }

            var result = new GetShipmentDTO
            {
                Id = shipment.Id,
                SenderId = shipment.SenderId,
                SenderName = shipment.Sender.FirstName + " " + shipment.Sender.LastName,
                ReceiverId = shipment.ReceiverId,
                ReceiverName = shipment.Receiver.FirstName + " " + shipment.Receiver.LastName,
                FromPostOfficeId = shipment.FromPostOfficeId,
                FromPostOfficeName = shipment.FromPostOffice.Name,
                ToPostOfficeId = shipment.ToPostOfficeId,
                ToPostOfficeName = shipment.ToPostOffice.Name,
                CreatedAt = shipment.CreatedAt,
                DeliveredAt = shipment.DeliveredAt,
                Status = shipment.Status,
                Parcel = (shipment.Parcel != null) ? new ParcelDTO
                {
                    Id = shipment.Parcel.Id,
                    Weight = shipment.Parcel.Weight,
                    Type = shipment.Parcel.Type,
                } : null,
                Price = shipment.Price,
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}


