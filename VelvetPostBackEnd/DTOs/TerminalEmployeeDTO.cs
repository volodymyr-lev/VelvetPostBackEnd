using VelvetPostBackEnd.Models;

namespace VelvetPostBackEnd.DTOs;

public class TerminalEmployeeDTO
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int TerminalId { get; set; }
    public EmployeeDTO Employee { get; set; }
    public TerminalDTO Terminal { get; set; }
}