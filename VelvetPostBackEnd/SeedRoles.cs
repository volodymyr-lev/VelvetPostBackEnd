using Microsoft.AspNetCore.Identity;
using System;
using VelvetPostBackEnd.Data;
using Microsoft.EntityFrameworkCore;
using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd
{
    public static class SeedRoles
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "Client", "PostOfficeEmployee", "TerminalEmployee" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // test admin:
            var adminEmail = "admin@velvetpost.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    PhoneNumber = "+380501234567",
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }


            if (!await context.Terminals.AnyAsync())
            {
                // Додавання тестового терміналу
                var terminal = new Terminal
                {
                    Name = "Центральний термінал",
                    Address = "вул. Центральна, 1",
                    
                    City = "Київ",
                    Type = "Основний"
                };

                context.Terminals.Add(terminal);
                await context.SaveChangesAsync();

                // Додавання тестового поштового відділення
                var postOffice = new PostOffice
                {
                    Name = "Відділення №1",
                    Address = "вул. Поштова, 10",
                    City = "Київ",
                    PhoneNumber = "+380442345678",
                    TerminalId = terminal.Id
                };

                context.PostOffices.Add(postOffice);
                await context.SaveChangesAsync();

                // Додавання тестового працівника
                var employee = new Employee
                {
                    FirstName = "Іван",
                    LastName = "Петренко",
                    Position = "Оператор",
                    PhoneNumber = "+380661234567",
                    Email = "operator@velvetpost.com",
                    StartDate = DateTime.UtcNow.AddMonths(-3)
                };

                context.Employees.Add(employee);
                await context.SaveChangesAsync();

                // Зв'язуємо працівника з поштовим відділенням
                var postOfficeEmployee = new PostOfficeEmployee
                {
                    EmployeeId = employee.Id,
                    PostOfficeId = postOffice.Id
                };

                context.PostOfficeEmployees.Add(postOfficeEmployee);
                await context.SaveChangesAsync();

                // Створення облікового запису для працівника
                var employeeUser = new ApplicationUser
                {
                    UserName = "operator@velvetpost.com",
                    Email = "operator@velvetpost.com",
                    EmailConfirmed = true,
                    PhoneNumber = employee.PhoneNumber,
                    PhoneNumberConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var employeeResult = await userManager.CreateAsync(employeeUser, "Employee123!");

                if (employeeResult.Succeeded)
                { 
                    employee.ApplicationUserId = employeeUser.Id;
                    context.Employees.Update(employee);
                    await context.SaveChangesAsync();

                    await userManager.AddToRoleAsync(employeeUser, "PostOfficeEmployee");
                }
            }
        }
    }
}
