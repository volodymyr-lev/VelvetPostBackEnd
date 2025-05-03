namespace VelvetPostBackEnd.DTOs;

public class ClientCreateShipmentDTO
{
    public int SenderId { get; set; }
    public int ReceiverId { get; set; }
    public int FromPostOfficeId { get; set; }
    public int ToPostOfficeId { get; set; }
}
