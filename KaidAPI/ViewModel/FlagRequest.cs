namespace KaidAPI.ViewModel;

public class FlagRequest
{
    public Guid ProjectId { get; set; }
    public string FlagDescription { get; set; }
    public string Status { get; set; }
    public string Reporter { get; set; }
    public int Priority { get; set; }
}


