using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.DTOs;

public class PostOfficeEmployeeDTO
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int PostOfficeId { get; set; }
    public EmployeeDTO Employee { get; set; }
    public PostOfficeDTO PostOffice { get; set; }
}
