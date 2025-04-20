using System.ComponentModel.DataAnnotations;
namespace VelvetPostBackEnd.Models;

public class Terminal
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    [StringLength(255)]
    public string Address { get; set; }

    [Required]
    [StringLength(100)]
    public string City { get; set; }
    
    [Required]
    public string Type { get; set; }

    // Навігаційні властивості
    public ICollection<PostOffice> PostOffices { get; set; }
    public ICollection<TerminalEmployee> TerminalEmployees { get; set; }
}