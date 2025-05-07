using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using VelvetPostBackEnd.Data;
using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.Controllers;
    
[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class DatabaseExportController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public DatabaseExportController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet("export")]
    public async Task<IActionResult> ExportDatabase()
    {
        try
        {
            var clientDtos = await _context.Clients
                .Select(c => new
                {
                    c.Id,
                    c.FirstName,
                    c.LastName,
                    c.PhoneNumber,
                    c.Email,
                    c.Address,
                    c.ApplicationUserId
                })
                .ToListAsync();

            var employeeDtos = await _context.Employees
                .Select(e => new
                {
                    e.Id,
                    e.FirstName,
                    e.LastName,
                    e.Position,
                    e.PhoneNumber,
                    e.Email,
                    e.StartDate,
                    e.ApplicationUserId
                })
                .ToListAsync();

            var terminalDtos = await _context.Terminals
                .Select(t => new
                {
                    t.Id,
                    t.Name,
                    t.Address,
                    t.City,
                    t.Type
                })
                .ToListAsync();

            var postOfficeDtos = await _context.PostOffices
                .Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Address,
                    p.City,
                    p.PhoneNumber,
                    p.TerminalId
                })
                .ToListAsync();

            var postOfficeEmployeeDtos = await _context.PostOfficeEmployees
                .Select(p => new
                {
                    p.Id,
                    p.EmployeeId,
                    p.PostOfficeId
                })
                .ToListAsync();

            var terminalEmployeeDtos = await _context.TerminalEmployees
                .Select(t => new
                {
                    t.Id,
                    t.EmployeeId,
                    t.TerminalId
                })
                .ToListAsync();

            var parcelDtos = await _context.Parcels
                .Select(p => new
                {
                    p.Id,
                    p.Weight,
                    p.Type
                })
                .ToListAsync();

            var shipmentDtos = await _context.Shipments
                .Select(s => new
                {
                    s.Id,
                    s.CreatedAt,
                    s.Status,
                    s.SenderId,
                    s.ReceiverId,
                    s.FromPostOfficeId,
                    s.ToPostOfficeId,
                    s.ParcelId
                })
                .ToListAsync();

            var databaseExport = new
            {
                Clients = clientDtos,
                Employees = employeeDtos,
                Terminals = terminalDtos,
                PostOffices = postOfficeDtos,
                PostOfficeEmployees = postOfficeEmployeeDtos,
                TerminalEmployees = terminalEmployeeDtos,
                Parcels = parcelDtos,
                Shipments = shipmentDtos
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };

            var jsonData = JsonConvert.SerializeObject(databaseExport, jsonSettings);

            byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(jsonData);
            string fileName = $"velvet_post_db_export_{DateTime.Now:yyyy-MM-dd}.json";

            return new FileContentResult(byteArray, "application/octet-stream")
            {
                FileDownloadName = fileName
            };
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = $"Error exporting database: {ex.Message}" });
        }
    }
}