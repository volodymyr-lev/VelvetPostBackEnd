using Microsoft.Identity.Client;

namespace VelvetPostBackEnd.DTOs;

public class GetClientDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string City { get; set; }
    public string Address { get; set; }
    public DateTime CreationDate { get; set; }
    public List<ClientShipmentDTO> Shipments { get; set; }
}


public class ClientShipmentDTO
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int FromPostOfficeId { get; set; }
    public int ToPostOfficeId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string Status { get; set; }
    public int? ParcelId { get; set; }
    public double Price { get; set; }
}