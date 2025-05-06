using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VelvetPostBackEnd.Data;
using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StatsController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ApplicationDbContext _context;

    public StatsController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _context = context;
    }

    [HttpGet("{id}")]
    [Authorize(Roles = "PostOfficeEmployee,Admin")]
    public async Task<IActionResult> GetPostOfficeStats(int id)
    {
        try
        {
            var postOffice = await _context.PostOffices.FindAsync(id);

            if (postOffice == null)
            {
                return NotFound("Відділення не знайдено");
            }

            var shipments = await _context.Shipments
                .Include(s=>s.Parcel)
                .Where(s => s.ToPostOfficeId == id || s.FromPostOfficeId == id).ToListAsync();

            var totalShipments = shipments.Count;

            var totalClients = shipments
                .SelectMany(s => new[] { s.SenderId, s.ReceiverId })
                .Distinct()
                .Count();

            var totalEmployees = await _context.PostOfficeEmployees
                    .Where(e => e.PostOfficeId == id)
                    .CountAsync();

            var deliveredShipments = shipments.Where(s => s.DeliveredAt != null).ToList();

            double averageDeliveryTime = deliveredShipments.Any()
                ? deliveredShipments.Average(s => (s.DeliveredAt.Value - s.CreatedAt).TotalHours)
                : 0;

            var lastYear =
                DateTime.UtcNow.AddYears(-1);

            Dictionary<int, string> monthTranslations = new Dictionary<int, string>
            {
                { 1, "Січ" },
                { 2, "Лют" },
                { 3, "Бер" },
                { 4, "Кві" },
                { 5, "Тра" },
                { 6, "Чер" },
                { 7, "Лип" },
                { 8, "Сер" },
                { 9, "Вер" },
                { 10, "Жов" },
                { 11, "Лис" },
                { 12, "Гру" }
            };

            var monthlyTrends = shipments
                .Where(s => s.CreatedAt >= lastYear)
                .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
                .Select(g => new MonthlyTrendsDTO
                {
                    Month = $"{monthTranslations[g.Key.Month]} {g.Key.Year}",
                    Count = g.Count()
                })
                .OrderBy(m => {
                    int year = int.Parse(m.Month.Split(' ')[1]);
                    int month = Array.FindIndex(monthTranslations.Values.ToArray(), x => x == m.Month.Split(' ')[0]) + 1;
                    return year * 12 + month;
                })
                .ToList();

            var shipmentTypes = new[] { "Посилка", "Бандероль", "Секограма", "Лист" };
            var totalParcels = shipments.Count(s => s.Parcel != null && (s.FromPostOfficeId == id || s.ToPostOfficeId ==id));
  
            var deliveryStatuses = shipments.Where(s => s.Status != null)
                .GroupBy(s => s.Status)
                .Select(g => new DeliveryStatusDTO
                {
                    Status = g.Key,
                    Count = g.Count()
                })
                .ToList();

            Dictionary<DayOfWeek, string> dayTranslations = new Dictionary<DayOfWeek, string>
            {
                { DayOfWeek.Monday, "Пн" },
                { DayOfWeek.Tuesday, "Вт" },
                { DayOfWeek.Wednesday, "Ср" },
                { DayOfWeek.Thursday, "Чт" },
                { DayOfWeek.Friday, "Пт" },
                { DayOfWeek.Saturday, "Сб" },
                { DayOfWeek.Sunday, "Нд" }
            };


            var stats = new StatsDTO
            {
                TotalShipments = totalShipments,
                TotalClients = totalClients,
                TotalEmployees = totalEmployees,
                AverageDeliveryTime = averageDeliveryTime,
                DailyShipments = shipments
                    .GroupBy(s => s.CreatedAt.Date)
                    .Select(g => new DailyShipmentDTO
                    {
                        Name = dayTranslations[g.Key.DayOfWeek], 
                        Sent = g.Count(s => s.FromPostOfficeId == id),
                        Received = g.Count(s => s.ToPostOfficeId == id)
                    })
                    .ToList(),
                MonthlyTrends = monthlyTrends,
                ShipmentsTypePercentage = shipmentTypes.Select(type =>
                {
                    int count = shipments
                        .Count(s => s.Parcel != null && s.Parcel.Type == type);

                    double percentage = totalParcels > 0
                        ? (double)count / totalParcels * 100
                        : 0;

                    return new ShipmentTypeDTO
                    {
                        name = type,  
                        value = Math.Round(percentage, 1) 
                    };
                }).ToList(),
                DeliveryStatuses = deliveryStatuses,
                Name = postOffice.Name,
                City = postOffice.City,
                Address = postOffice.Address
            };

            return Ok(stats);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Cталася помилка при отриманні статистики {ex.Message}");
        }
    }
}




public class StatsDTO
{
    public int TotalShipments { get; set; }
    public int TotalClients { get; set; }
    public int TotalEmployees { get; set; }
    public double AverageDeliveryTime { get; set; }
    public List<DailyShipmentDTO> DailyShipments { get; set; }
    public List<MonthlyTrendsDTO> MonthlyTrends { get; set; }
    public List<ShipmentTypeDTO> ShipmentsTypePercentage { get; set; }
    public List<DeliveryStatusDTO> DeliveryStatuses { get; set; }
    public string Name { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
}

public class ShipmentTypeDTO
{
    public string name { get; set; }  
    public double value { get; set; }  
}

public class DailyShipmentDTO
{
    public string Name { get; set; }
    public int Sent { get; set; }
    public int Received { get; set; }
}

public class DeliveryStatusDTO
{
    public string Status { get; set; }
    public int Count { get; set; }
}

public class MonthlyTrendsDTO
{
    public string Month { get; set; }
    public int Count { get; set; }
}