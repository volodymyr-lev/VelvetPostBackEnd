namespace VelvetPostBackEnd.DTOs;

public class GetShipmentDTO
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int FromPostOfficeId { get; set; }
    public int ToPostOfficeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string Status { get; set; }
    public ParcelDTO Parcel { get; set; }
    public double Price { get; set; }
}

public class ParcelDTO
{
    public int Id { get; set; }
    public decimal Weight { get; set; }
    public string Type { get; set; }
}
