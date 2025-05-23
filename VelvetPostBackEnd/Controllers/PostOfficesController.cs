﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using VelvetPostBackEnd.Data;
using VelvetPostBackEnd.DTOs;
using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PostOfficesController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public PostOfficesController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,PostOfficeEmployee,Client")]
    public async Task<IActionResult> GetPostOffices()
    {
        try
        {
            var postOffices = await _context.PostOffices.ToListAsync();
            return Ok(postOffices);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при отриманні відділень {ex.Message}");
        }
    }
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,PostOfficeEmployee,Client")]
    public async Task<IActionResult> GetPostOffices(int id)
    {
        try
        {
            var postOffice = await _context.PostOffices.FindAsync(id);
            if (postOffice == null)
            {
                return NotFound("Відділення не знайдено");
            }
            int? inCount = _context.Shipments.Count(s => (s.ToPostOfficeId == id && s.Status != "Доставлено"));
            int? outCount = _context.Shipments.Count(s => (s.FromPostOfficeId == id && s.Status != "Доставлено"));
            return Ok(new PostOfficeDTO
            {
                Id = postOffice.Id,
                Name = postOffice.Name,
                Address = postOffice.Address,
                City = postOffice.City,
                PhoneNumber = postOffice.PhoneNumber,
                TerminalId = postOffice.TerminalId,
                EmployeeCount = _context.PostOfficeEmployees.Count(e => e.PostOfficeId == id),
                IncomingShipmentsCount = inCount,
                OutgoingShipmentsCount = outCount

            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при отриманні відділень {ex.Message}");
        }
    }

    [HttpGet("{id}/shipments")]
    [Authorize(Roles = "Admin,PostOfficeEmployee")]
    public async Task<IActionResult> GetPostOfficeShipments(int id)
    {
        try
        {
            var po = await _context.PostOffices.FindAsync(id);

            if (po == null)
            {
                return NotFound("Відділення не знайдено");
            }

            var shipments = await _context.Shipments
                .Where(s => s.ToPostOfficeId == id)
                .Include(s=>s.FromPostOffice)
                .Include(s=>s.ToPostOffice)
                .Include(s => s.Parcel)
                .Include(s => s.Sender)
                .Include(s => s.Receiver)
                .ToListAsync();

            var result = shipments.Select(s => new GetShipmentDTO
            {
                Id = s.Id,
                SenderId = s.SenderId,
                SenderName = s.Sender.FirstName + " " + s.Sender.LastName,
                SenderAddress = s.Sender.Address,
                ReceiverId = s.ReceiverId,
                ReceiverName = s.Receiver.FirstName + " " + s.Receiver.LastName,
                ReceiverAddress = s.Receiver.Address,
                FromPostOfficeId = s.FromPostOfficeId,
                FromPostOfficeName = s.FromPostOffice.Name,
                ToPostOfficeId = s.ToPostOfficeId,
                ToPostOfficeName = s.ToPostOffice.Name,
                CreatedAt = s.CreatedAt,
                DeliveredAt = s.DeliveredAt,
                Status = s.Status,
                Parcel = (s.Parcel != null) ? new ParcelDTO
                {
                    Id = s.Parcel.Id,
                    Weight = s.Parcel.Weight,
                    Type = s.Parcel.Type,
                } : null,
                Price = s.Price
            }).ToList();


            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при отриманні відправлень з відділення {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePostOffice(int id, [FromBody] PostOfficeDTO UpdatedPostOffice)
    {
        var existingPostOffice = await _context.PostOffices.FindAsync(id);
        if (existingPostOffice == null)
        {
            return NotFound("Відділення не знайдено");
        }

        existingPostOffice.Name = UpdatedPostOffice.Name;
        existingPostOffice.Address = UpdatedPostOffice.Address;
        existingPostOffice.City = UpdatedPostOffice.City;
        existingPostOffice.PhoneNumber = UpdatedPostOffice.PhoneNumber;
        existingPostOffice.TerminalId = UpdatedPostOffice.TerminalId;

        try
        {
            await _context.SaveChangesAsync();
            return Ok(existingPostOffice);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Помилка при оновленні: {ex.Message}");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePostOffice(int id)
    {
        var postOffice = await _context.PostOffices.FindAsync(id);
        if (postOffice == null)
        {
            return NotFound("Відділення не знайдено");
        }

        try
        {
            _context.PostOffices.Remove(postOffice);
            await _context.SaveChangesAsync();
            return Ok($"Відділення з ID {id} успішно видалено");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Помилка при видаленні: {ex.Message}");
        }
    }

    [HttpPost("create")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreatePostOffice(PostOfficeDTO postOffice)
    {
        if (ModelState.IsValid)
        {
            if (postOffice.TerminalId >= 0 && _context.Terminals.Any(t => t.Id == postOffice.TerminalId))
            {
                var po = new PostOffice
                {
                    TerminalId = postOffice.TerminalId,
                    Name = postOffice.Name,
                    Address = postOffice.Address,
                    City = postOffice.City,
                    PhoneNumber = postOffice.PhoneNumber,
                };

                _context.PostOffices.Add(po);
                _context.SaveChangesAsync();
                return Ok($"Відділення {po.Name} успішно створено");
            }

            return BadRequest("Терміналу з заданим ID не існує");
        }
        return BadRequest("Спробуйте пізніше");
    }
}




