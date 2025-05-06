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
                                         .Include(s => s.Sender)
                                         .Include(s => s.Receiver)
                                         .Include(s => s.FromPostOffice)
                                         .Include(s => s.ToPostOffice)
                                         .FirstAsync(s => s.Id == id);

            if (shipment == null)
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


    [HttpPut("{id}")]
    public async Task<IActionResult> EditShipment(int id, [FromBody] EditShipmentDTO editShipmentDTO)
    {
        try
        {
            var shipment = await _context.Shipments
                                         .Include(s => s.Parcel)
                                         .FirstOrDefaultAsync(s => s.Id == id);
            if (shipment == null)
            {
                return NotFound($"No shipment with id = {id}.");
            }

            shipment.SenderId = editShipmentDTO.SenderId;
            shipment.ReceiverId = editShipmentDTO.ReceiverId;
            shipment.FromPostOfficeId = editShipmentDTO.FromPostOfficeId;
            shipment.ToPostOfficeId = editShipmentDTO.ToPostOfficeId;
            shipment.CreatedAt = editShipmentDTO.CreatedAt;
            shipment.DeliveredAt = editShipmentDTO.DeliveredAt;
            shipment.Status = editShipmentDTO.Status;
            shipment.Price = editShipmentDTO.Price;


            if (shipment.Parcel == null 
                && !string.IsNullOrEmpty(editShipmentDTO.ParcelType)
                && editShipmentDTO.Status == "Очікує пакунок")
            {
                var parcel = new Parcel()
                {
                    Weight = editShipmentDTO.ParcelWeight ?? 0,
                    Type = editShipmentDTO.ParcelType
                };

                _context.Parcels.Add(parcel);
                shipment.Status = "Очікує відправки";
                shipment.Parcel = parcel;
                shipment.ParcelId = parcel.Id;
            }
            else
            {
                shipment.ParcelId = editShipmentDTO.ParcelId;
                if (editShipmentDTO.ParcelId == null) shipment.Status = "Очікує пакунок";
                else if (editShipmentDTO.ParcelId != null) shipment.Status = "Очікує відправки";

                if (shipment.Parcel != null)
                {
                    shipment.Parcel.Weight = editShipmentDTO.ParcelWeight ?? shipment.Parcel.Weight;
                    shipment.Parcel.Type = editShipmentDTO.ParcelType ?? shipment.Parcel.Type;
                }
            }

            var result = await _context.SaveChangesAsync();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles ="PostOfficeEmployee,Admin")]
    public async Task<IActionResult> ChangeShipmentStatus(int id, [FromBody] ChangeShipmentStatusDTO StatusDTO)
    {
        try
        {
            var shipment = await _context.Shipments.FindAsync(id);
            if (shipment == null)
            {
                return BadRequest($"Відпарвлення {id} не знайдено");
            }

            shipment.Status = StatusDTO.Status;

            if(StatusDTO.Status == "Доставлено")
            {
                shipment.DeliveredAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return Ok("Статус змінено");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}

public class EditShipmentDTO
{
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int FromPostOfficeId { get; set; }
    public int ToPostOfficeId { get; set; }
    public int? ParcelId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string Status { get; set; }
    public double Price { get; set; }
    public decimal? ParcelWeight { get; set; }
    public string? ParcelType { get; set; }
}
