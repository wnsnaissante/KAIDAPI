using System.ComponentModel.DataAnnotations;

namespace KaidAPI.Models;

public class User
{
    [Key]
    public Guid UserId { get; set; }
    
    [Required]
    [MaxLength(50)]
    public string Username { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Email { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    [MaxLength(255)]
    public string? AuthentikIssuer { get; set; }
    [MaxLength(255)]
    public string? AuthentikSubject { get; set; }
}