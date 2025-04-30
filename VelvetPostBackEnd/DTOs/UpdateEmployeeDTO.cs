namespace VelvetPostBackEnd.DTOs;

public class UpdateEmployeeDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string Position { get; set; }
    public int DepId { get; set; }
}
