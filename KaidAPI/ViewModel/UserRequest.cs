using System.ComponentModel.DataAnnotations;

namespace KaidAPI.ViewModel;

public class UserRequest
{
    [Required]
    public string Email { get; set; }

    public string? Username { get; set; }

    [Required]
    public string AuthentikIssuer { get; set; }

    [Required]
    public string AuthentikSubject { get; set; }
}