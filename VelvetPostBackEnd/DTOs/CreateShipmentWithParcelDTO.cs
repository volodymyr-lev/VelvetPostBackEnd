namespace VelvetPostBackEnd.DTOs;


public class CreateShipmentWithParcelDTO
{
    public string FromCity { get; set; }
    public int FromOffice { get; set; }
    public string ToCity { get; set; }
    public int ToOffice { get; set; }
    public string RecipientPhone {  get; set; }
    public string SenderPhone { get; set; }
    public decimal Weight { get; set; }
    public string Type { get; set; }
    public double Price {  get; set; }
}
