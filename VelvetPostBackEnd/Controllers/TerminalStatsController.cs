using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VelvetPostBackEnd.Models;
using VelvetPostBackEnd.DTOs;
using VelvetPostBackEnd.Data;
using Microsoft.AspNetCore.Authorization;

namespace VelvetPostBackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TerminalShipmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TerminalShipmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("Overview/{id}")]
        [Authorize(Roles = "Admin,TerminalEmployee")]
        public async Task<ActionResult<TerminalOverviewDTO>> GetTerminalOverview(int id)
        {
            // Отримуємо базову інформацію про термінал
            var terminal = await _context.Terminals
                .Include(t => t.PostOffices)
                .Include(t => t.TerminalEmployees)
                    .ThenInclude(te => te.Employee)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (terminal == null)
            {
                return NotFound("Термінал не знайдено");
            }

            var terminalData = new TerminalDataDTO
            {
                Id = terminal.Id,
                Name = terminal.Name,
                Address = terminal.Address,
                City = terminal.City,
                Type = terminal.Type,
                PostOffices = terminal.PostOffices.Select(po => new PostOfficeDTO
                {
                    Id = po.Id,
                    Name = po.Name,
                    Address = po.Address,
                    City = po.City,
                    PhoneNumber = po.PhoneNumber
                }).ToList()
            };

            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-7);

            var shipments = await _context.Shipments
                .Include(s => s.Parcel)
                .Include(s => s.FromPostOffice)
                .Include(s => s.ToPostOffice)
                .Where(s =>
                    s.CreatedAt >= startDate &&
                    s.CreatedAt <= endDate &&
                    (s.FromPostOffice.TerminalId == id || s.ToPostOffice.TerminalId == id))
                .ToListAsync();

            var shipmentData = Enumerable.Range(0, 7)
                .Select(i => startDate.AddDays(i).Date)
                .Select(date => new ShipmentDataDTO
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Incoming = shipments.Count(s =>
                        s.ToPostOffice.TerminalId == id &&
                        s.CreatedAt.Date == date),
                    Outgoing = shipments.Count(s =>
                        s.FromPostOffice.TerminalId == id &&
                        s.CreatedAt.Date == date)
                })
                .ToList();

            var parcels = await _context.Parcels
                .Include(p => p.Shipment)
                .Where(p =>
                    p.Shipment.CreatedAt >= startDate &&
                    p.Shipment.CreatedAt <= endDate &&
                    (p.Shipment.FromPostOffice.TerminalId == id || p.Shipment.ToPostOffice.TerminalId == id))
                .ToListAsync();

            var shipmentTypeData = new List<TerminalShipmentTypeDTO>
            {
                new TerminalShipmentTypeDTO { Name = "Лист", Value = parcels.Count(p => p.Type == "Лист") },
                new TerminalShipmentTypeDTO { Name = "Секограма", Value = parcels.Count(p => p.Type == "Секограма") },
                new TerminalShipmentTypeDTO { Name = "Бандероль", Value = parcels.Count(p => p.Type == "Бандероль") },
                new TerminalShipmentTypeDTO { Name = "Посилка", Value = parcels.Count(p => p.Type == "Посилка") },
            };


            var statusData = new List<StatusDataDTO>
            {
                new StatusDataDTO { Name = "Очікує відправки", Value = shipments.Count(s => s.Status == "Очікує відправки") },
                new StatusDataDTO { Name = "В дорозі", Value = shipments.Count(s => s.Status == "В дорозі") },
                new StatusDataDTO { Name = "Доставлено", Value = shipments.Count(s => s.Status == "Доставлено") },
                new StatusDataDTO { Name = "Очікує отримувача", Value = shipments.Count(s => s.Status == "Очікує отримувача") }
            };

            var timeDistribution = GenerateTimeDistribution(shipments, id);

            var deliveryTargets = GenerateDeliveryTargets(shipments, id);

            var parcelAnalysis = parcels.Select(p => new ParcelAnalysisDTO
            {
                Id = p.Id,
                Weight = (double)p.Weight,
                Price = shipments.FirstOrDefault(s=>s.ParcelId == p.Id).Price,
                Type = shipments.FirstOrDefault(s => s.ParcelId == p.Id).Parcel.Type,
            }).ToList();

            var response = new TerminalOverviewDTO
            {
                TerminalData = terminalData,
                ShipmentData = shipmentData,
                ShipmentTypeData = shipmentTypeData,
                StatusData = statusData,
                TimeDistribution = timeDistribution,
                DeliveryTargets = deliveryTargets,
                ParcelAnalysis = parcelAnalysis
            };

            return response;
        }

        #region Допоміжні методи

        private List<TimeDistributionDTO> GenerateTimeDistribution(List<Shipment> shipments, int terminalId)
        {
            var timeSlots = new[]
            {
                "20:00-22:00", "22:00-00:00", "00:00-02:00",
                "02:00-04:00", "04:00-06:00", "06:00-08:00",
                "08:00-10:00", "10:00-12:00", "12:00-14:00",
                "14:00-16:00", "16:00-18:00", "18:00-20:00"
            };

            var result = new List<TimeDistributionDTO>();

            foreach (var slot in timeSlots)
            {
                var times = slot.Split('-');
                var startHour = int.Parse(times[0].Split(':')[0]);
                var endHour = int.Parse(times[1].Split(':')[0]);

                var sentCount = shipments.Count(s =>
                    s.CreatedAt.Hour >= startHour &&
                    s.CreatedAt.Hour < endHour &&
                    s.FromPostOffice.TerminalId == terminalId);

                var receivedCount = shipments.Count(s =>
                    s.CreatedAt.Hour >= startHour &&
                    s.CreatedAt.Hour < endHour &&
                    s.ToPostOffice.TerminalId == terminalId);

                result.Add(new TimeDistributionDTO
                {
                    Name = slot,
                    Sent = sentCount,
                    Received = receivedCount
                });
            }

            return result;
        }

        private List<DeliveryTargetDTO> GenerateDeliveryTargets(List<Shipment> shipments, int terminalId)
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-7);
            var result = new List<DeliveryTargetDTO>();

            for (int i = 0; i < 7; i++)
            {
                var date = startDate.AddDays(i).Date;

                var planned = 100;

                var actual = shipments.Count(s =>
                    s.CreatedAt.Date == date &&
                    (s.FromPostOffice.TerminalId == terminalId || s.ToPostOffice.TerminalId == terminalId));

                result.Add(new DeliveryTargetDTO
                {
                    Date = date.ToString("yyyy-MM-dd"),
                    Plan = planned,
                    Fact = actual
                });
            }

            return result;
        }

        #endregion
    }
}