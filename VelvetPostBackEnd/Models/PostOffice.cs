using System.ComponentModel.DataAnnotations;

namespace VelvetPostBackEnd.Models;


public class PostOffice
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(255)]
    public string Address { get; set; }

    [Required]
    [StringLength(100)]
    public string City { get; set; }

    [Required]
    [StringLength(20)]
    public string PhoneNumber { get; set; }

    // Зовнішній ключ
    public int? TerminalId { get; set; }

    // Навігаційні властивості
    public Terminal Terminal { get; set; }
    public ICollection<PostOfficeEmployee> PostOfficeEmployees { get; set; }
    public ICollection<Shipment> OutgoingShipments { get; set; }
    public ICollection<Shipment> IncomingShipments { get; set; }
}