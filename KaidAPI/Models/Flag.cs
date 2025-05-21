namespace KaidAPI.Models;

public class Flag
{
    public Guid FlagId { get; set; }
    public Guid TeamId { get; set; }
    public string FlagDescription { get; set; }
    public string Status { get; set; }
    public string Reporter { get; set; }
    public DateTime CreatedAt { get; set; }
    public int Priority { get; set; }
}
