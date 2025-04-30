using System.ComponentModel.DataAnnotations;
namespace VelvetPostBackEnd.Models;

public class Shipment
{
    public int Id { get; set; }
    public double Price { get; set; }

    // Зовнішні ключі
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int FromPostOfficeId { get; set; }
    public int ToPostOfficeId { get; set; }
    public int? ParcelId { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public DateTime? DeliveredAt { get; set; }

    [Required]
    public string Status { get; set; }

    // Навігаційні властивості
    public Client Sender { get; set; }
    public Client Receiver { get; set; }
    public PostOffice FromPostOffice { get; set; }
    public PostOffice ToPostOffice { get; set; }
    public Parcel Parcel { get; set; }
}