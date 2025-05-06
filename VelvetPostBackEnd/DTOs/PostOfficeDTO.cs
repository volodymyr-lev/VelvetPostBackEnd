using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.DTOs;


public class PostOfficeDTO
{
    public int? Id { get; set; }
    public string Name {  get; set; }
    public string Address {  get; set; }
    public string City { get; set; }
    public string PhoneNumber { get; set; }
    public int? TerminalId { get; set; }

    public int? EmployeeCount { get; set; }
    public int? OutgoingShipmentsCount { get; set; }
    public int? IncomingShipmentsCount { get; set; }
}
