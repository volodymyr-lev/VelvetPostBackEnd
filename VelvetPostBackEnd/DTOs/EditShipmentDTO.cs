namespace VelvetPostBackEnd.DTOs;

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
