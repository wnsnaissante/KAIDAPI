using System.ComponentModel.DataAnnotations;

namespace KaidAPI.Models;

public class User
{
    [Key]
    public string UserID { get; set; }
    
    [Required]
    [MaxLength(20)]
    public string Username { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; }
    
    public DateTime CreatedAt { get; set; }
}