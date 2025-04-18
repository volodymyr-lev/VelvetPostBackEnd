using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace VelvetPostBackEnd.Models;


public class Parcel
{
    public int Id { get; set; }

    [Required]
    [Column(TypeName = "decimal(5,2)")]
    public decimal Weight { get; set; }

    [Required]
    public string Type { get; set; }

    // Навігаційні властивості
    public Shipment Shipment { get; set; }
}