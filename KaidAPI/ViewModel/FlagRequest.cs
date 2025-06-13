using KaidAPI.Models;

namespace KaidAPI.ViewModel;

public class FlagRequest
{
    public Guid ProjectId { get; set; }
    public string FlagDescription { get; set; }
    public FlagStatus Status { get; set; }
    public int Priority { get; set; }
    public Guid? TeamId { get; set; }
}


