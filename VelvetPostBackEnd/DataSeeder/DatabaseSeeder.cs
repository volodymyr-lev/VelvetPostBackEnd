using Bogus;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VelvetPostBackEnd.Data;
using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.Seeders
{
    public class DatabaseSeeder
    {
        private readonly ILogger<DatabaseSeeder> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly Faker _faker;

        private const int POST_OFFICE_COUNT = 25;
        private const int CLIENT_COUNT = 1000;
        private const int POST_OFFICE_EMPLOYEES_PER_OFFICE = 4;
        private const int TERMINAL_COUNT = 10;
        private const int TERMINAL_EMPLOYEES_PER_TERMINAL = 10;
        private const int SHIPMENT_COUNT = 1000;
        private const int SHIPMENTS_WITHOUT_PARCELS = 50;

        private readonly string[] _shipmentStatuses = {
            "Очікує пакунок", "Очікує відправки", "В дорозі", "Очікує отримувача", "Доставлено"
        };

        private readonly string[] _parcelTypes = {
            "Посилка", "Лист", "Секограма", "Бандероль"
        };

        private readonly string[] _postOfficePositions = {
            "Менеджер", "Оператор", "Кур'єр"
        };

        private readonly string[] _terminalPositions = {
            "Менеджер", "Вантажник"
        };

        private readonly string[] _terminalTypes = {
            "Основний", "Змішаний"
        };

        private readonly string[] _ukrainianCities = {
            "Київ", "Львів", "Одеса", "Харків", "Дніпро", "Запоріжжя", "Вінниця",
            "Чернівці", "Івано-Франківськ", "Тернопіль", "Луцьк", "Рівне", "Житомир",
            "Ужгород", "Кропивницький", "Черкаси", "Чернігів", "Суми", "Полтава", "Миколаїв",
            "Херсон", "Хмельницький", "Маріуполь", "Кривий Ріг", "Біла Церква"
        };

        private const string CLIENT_PASSWORD = "Client123!";
        private const string EMPLOYEE_PASSWORD = "Employee123!";

        public DatabaseSeeder(
            ILogger<DatabaseSeeder> logger,
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _faker = new Faker("uk");
        }

        public async Task SeedAsync()
        {
            _logger.LogInformation("Starting database seeding...");

            await EnsureRolesCreatedAsync();

            await ClearExistingDataAsync();

            var terminals = await CreateTerminalsAsync();

            var postOffices = await CreatePostOfficesAsync(terminals);

            var clients = await CreateClientsAsync();

            var postOfficeEmployees = await CreatePostOfficeEmployeesAsync(postOffices);

            var terminalEmployees = await CreateTerminalEmployeesAsync(terminals);

            await CreateParcelsAndShipmentsAsync(clients, postOffices);

            _logger.LogInformation("Database seeding completed successfully.");
        }

        private async Task EnsureRolesCreatedAsync()
        {
            string[] roles = { "Admin", "Client", "PostOfficeEmployee", "TerminalEmployee" };

            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                    _logger.LogInformation($"Created role: {role}");
                }
            }
        }

        private async Task ClearExistingDataAsync()
        {
            _logger.LogWarning("Clearing existing data...");

            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Shipments\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Parcels\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PostOfficeEmployees\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"TerminalEmployees\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"PostOffices\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Terminals\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Employees\" CASCADE");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Clients\" CASCADE");

            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetUserRoles\"");
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM \"AspNetUsers\"");

            var tableNames = new[] {
                "Clients", "Employees", "Terminals", "PostOffices",
                "PostOfficeEmployees", "TerminalEmployees", "Parcels", "Shipments"
            };

            foreach (var tableName in tableNames)
            {
                await _context.Database.ExecuteSqlRawAsync($"ALTER SEQUENCE \"{tableName}_Id_seq\" RESTART WITH 1");
            }

            await _context.SaveChangesAsync();
        }

        private async Task<List<Terminal>> CreateTerminalsAsync()
        {
            _logger.LogInformation($"Creating {TERMINAL_COUNT} terminals...");

            var randomCities = _ukrainianCities.OrderBy(x => Guid.NewGuid()).Take(TERMINAL_COUNT).ToArray();
            var terminals = new List<Terminal>();

            for (int i = 0; i < TERMINAL_COUNT; i++)
            {
                var city = randomCities[i];
                var terminal = new Terminal
                {
                    Name = $"Термінал {city}",
                    Address = $"вул. {_faker.Address.StreetName()}, {_faker.Random.Number(1, 100)}",
                    City = city,
                    Type = _faker.PickRandom(_terminalTypes)
                };

                terminals.Add(terminal);
            }

            await _context.Terminals.AddRangeAsync(terminals);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {terminals.Count} terminals");
            return terminals;
        }

        private async Task<List<PostOffice>> CreatePostOfficesAsync(List<Terminal> terminals)
        {
            _logger.LogInformation($"Creating {POST_OFFICE_COUNT} post offices...");

            var postOffices = new List<PostOffice>();
            var officesPerTerminalCity = POST_OFFICE_COUNT / TERMINAL_COUNT;
            var remainingOffices = POST_OFFICE_COUNT % TERMINAL_COUNT;

            foreach (var terminal in terminals)
            {
                var officeCount = officesPerTerminalCity + (remainingOffices > 0 ? 1 : 0);
                if (remainingOffices > 0) remainingOffices--;

                for (int i = 0; i < officeCount; i++)
                {
                    var postOffice = new PostOffice
                    {
                        Name = $"Відділення №{_faker.Random.Number(1, 999)} {terminal.City}",
                        Address = $"вул. {_faker.Address.StreetName()}, {_faker.Random.Number(1, 100)}",
                        City = terminal.City,
                        PhoneNumber = $"+380{_faker.Random.Number(100000000, 999999999)}",
                        TerminalId = terminal.Id
                    };

                    postOffices.Add(postOffice);
                }
            }

            await _context.PostOffices.AddRangeAsync(postOffices);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {postOffices.Count} post offices");
            return postOffices;
        }

        private async Task<List<Client>> CreateClientsAsync()
        {
            _logger.LogInformation($"Creating {CLIENT_COUNT} clients...");

            var clients = new List<Client>();
            var fakerClient = new Faker<Client>("uk")
                .RuleFor(c => c.FirstName, f => f.Name.FirstName())
                .RuleFor(c => c.LastName, f => f.Name.LastName())
                .RuleFor(c => c.PhoneNumber, f => $"+380{f.Random.Number(100000000, 999999999)}")
                .RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.FirstName, c.LastName).ToLower())
                .RuleFor(c => c.City, f => f.PickRandom(_ukrainianCities))
                .RuleFor(c => c.Address, f => $"вул. {f.Address.StreetName()}, {f.Random.Number(1, 100)}, кв. {f.Random.Number(1, 100)}");

            for (int i = 0; i < CLIENT_COUNT; i++)
            {
                var client = fakerClient.Generate();

                var userName = $"client_{_faker.Random.AlphaNumeric(8)}";
                var user = new ApplicationUser
                {
                    UserName = userName,
                    Email = client.Email,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, CLIENT_PASSWORD);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, "Client");
                    client.ApplicationUserId = user.Id;
                    client.ApplicationUser = user;
                    clients.Add(client);
                }
                else
                {
                    _logger.LogError($"Failed to create user for client {client.FirstName} {client.LastName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
            }

            await _context.Clients.AddRangeAsync(clients);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {clients.Count} clients");
            return clients;
        }

        private async Task<List<Employee>> CreatePostOfficeEmployeesAsync(List<PostOffice> postOffices)
        {
            int totalEmployees = postOffices.Count * POST_OFFICE_EMPLOYEES_PER_OFFICE;
            _logger.LogInformation($"Creating {totalEmployees} post office employees...");

            var employees = new List<Employee>();

            foreach (var postOffice in postOffices)
            {
                for (int i = 0; i < POST_OFFICE_EMPLOYEES_PER_OFFICE; i++)
                {
                    var firstName = _faker.Name.FirstName();
                    var lastName = _faker.Name.LastName();
                    var email = _faker.Internet.Email(firstName, lastName).ToLower();
                    var position = _postOfficePositions[i % _postOfficePositions.Length];

                    var employee = new Employee
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Position = position,
                        PhoneNumber = $"+380{_faker.Random.Number(100000000, 999999999)}",
                        Email = email,
                        StartDate = _faker.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow)
                    };

                    var userName = $"employee_{_faker.Random.AlphaNumeric(8)}";
                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = email,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, EMPLOYEE_PASSWORD);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "PostOfficeEmployee");
                        employee.ApplicationUserId = user.Id;
                        employee.ApplicationUser = user;

                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();

                        var postOfficeEmployee = new PostOfficeEmployee
                        {
                            EmployeeId = employee.Id,
                            PostOfficeId = postOffice.Id,
                            Employee = employee,
                            PostOffice = postOffice
                        };

                        _context.PostOfficeEmployees.Add(postOfficeEmployee);
                        await _context.SaveChangesAsync();

                        employees.Add(employee);
                    }
                    else
                    {
                        _logger.LogError($"Failed to create user for employee {firstName} {lastName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            _logger.LogInformation($"Created {employees.Count} post office employees");
            return employees;
        }

        private async Task<List<Employee>> CreateTerminalEmployeesAsync(List<Terminal> terminals)
        {
            int totalEmployees = terminals.Count * TERMINAL_EMPLOYEES_PER_TERMINAL;
            _logger.LogInformation($"Creating {totalEmployees} terminal employees...");

            var employees = new List<Employee>();

            foreach (var terminal in terminals)
            {
                for (int i = 0; i < TERMINAL_EMPLOYEES_PER_TERMINAL; i++)
                {
                    var firstName = _faker.Name.FirstName();
                    var lastName = _faker.Name.LastName();
                    var email = _faker.Internet.Email(firstName, lastName).ToLower();
                    var position = _terminalPositions[i % _terminalPositions.Length];

                    var employee = new Employee
                    {
                        FirstName = firstName,
                        LastName = lastName,
                        Position = position,
                        PhoneNumber = $"+380{_faker.Random.Number(100000000, 999999999)}",
                        Email = email,
                        StartDate = _faker.Date.Between(DateTime.UtcNow.AddYears(-5), DateTime.UtcNow)
                    };

                    var userName = $"terminal_{_faker.Random.AlphaNumeric(8)}";
                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = email,
                        CreatedAt = DateTime.UtcNow
                    };

                    var result = await _userManager.CreateAsync(user, EMPLOYEE_PASSWORD);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(user, "TerminalEmployee");
                        employee.ApplicationUserId = user.Id;
                        employee.ApplicationUser = user;

                        _context.Employees.Add(employee);
                        await _context.SaveChangesAsync();

                        var terminalEmployee = new TerminalEmployee
                        {
                            EmployeeId = employee.Id,
                            TerminalId = terminal.Id,
                            Employee = employee,
                            Terminal = terminal
                        };

                        _context.TerminalEmployees.Add(terminalEmployee);
                        await _context.SaveChangesAsync();

                        employees.Add(employee);
                    }
                    else
                    {
                        _logger.LogError($"Failed to create user for terminal employee {firstName} {lastName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    }
                }
            }

            _logger.LogInformation($"Created {employees.Count} terminal employees");
            return employees;
        }

        private async Task CreateParcelsAndShipmentsAsync(List<Client> clients, List<PostOffice> postOffices)
        {
            _logger.LogInformation($"Creating {SHIPMENT_COUNT} shipments ({SHIPMENTS_WITHOUT_PARCELS} without parcels)...");

            var random = new Random();
            var shipments = new List<Shipment>();

            for (int i = 0; i < SHIPMENT_COUNT; i++)
            {
                var sender = clients[random.Next(clients.Count)];
                var receiver = clients[random.Next(clients.Count)];

                while (sender.Id == receiver.Id)
                {
                    receiver = clients[random.Next(clients.Count)];
                }

                var fromOffice = postOffices[random.Next(postOffices.Count)];
                var toOffice = postOffices[random.Next(postOffices.Count)];

                var shipment = new Shipment
                {
                    SenderId = sender.Id,
                    ReceiverId = receiver.Id,
                    FromPostOfficeId = fromOffice.Id,
                    ToPostOfficeId = toOffice.Id,
                    CreatedAt = _faker.Date.Between(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow),
                    Status = _faker.PickRandom(_shipmentStatuses),
                    Price = Math.Round(_faker.Random.Double(50, 500), 2)
                };

                if (shipment.Status == "Доставлено")
                {
                    shipment.DeliveredAt = shipment.CreatedAt.AddDays(_faker.Random.Number(1, 10));
                }

                bool hasParcel = i >= SHIPMENTS_WITHOUT_PARCELS;

                if (hasParcel)
                {
                    var parcel = new Parcel
                    {
                        Weight = (decimal)_faker.Random.Double(0.1, 20),
                        Type = _faker.PickRandom(_parcelTypes)
                    };

                    _context.Parcels.Add(parcel);
                    await _context.SaveChangesAsync();

                    shipment.ParcelId = parcel.Id;
                }

                shipments.Add(shipment);
            }

            await _context.Shipments.AddRangeAsync(shipments);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created {shipments.Count} shipments");
        }
    }
}