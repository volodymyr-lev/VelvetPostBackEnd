using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace VelvetPostBackEnd.Models;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAt { get; set; }

    public Client Client { get; set; }
    public Employee Employee { get; set; }
}

public class Client
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public string LastName { get; set; }

    [Required]
    [StringLength(100)]
    public string PhoneNumber { get; set; }

    [StringLength(100)]
    public string Email { get; set; }

    [StringLength(100)]
    public string City { get; set; }

    [StringLength(100)]
    public string Address { get; set; }

    // Зв'язок з Identity (опціонально)
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    // Навігаційні властивості
    public ICollection<Shipment> SentShipments { get; set; }
    public ICollection<Shipment> ReceivedShipments { get; set; }
}

public class Employee
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string FirstName { get; set; }

    [Required]
    [StringLength(100)]
    public string LastName { get; set; }

    [Required]
    public string Position { get; set; }

    [Required]
    [StringLength(100)]
    public string PhoneNumber { get; set; }

    [StringLength(100)]
    public string Email { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    // Зв'язок з Identity (опціонально)
    public string ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; }

    // Навігаційні властивості
    public PostOfficeEmployee PostOfficeEmployee { get; set; }
    public TerminalEmployee TerminalEmployee { get; set; }
}


public class PostOfficeEmployee
{
    public int Id { get; set; }

    // Зовнішні ключі
    public int EmployeeId { get; set; }
    public int PostOfficeId { get; set; }

    // Навігаційні властивості
    public Employee Employee { get; set; }
    public PostOffice PostOffice { get; set; }
}


public class TerminalEmployee
{
    public int Id { get; set; }

    // Зовнішні ключі
    public int EmployeeId { get; set; }
    public int TerminalId { get; set; }

    // Навігаційні властивості
    public Employee Employee { get; set; }
    public Terminal Terminal { get; set; }
}
