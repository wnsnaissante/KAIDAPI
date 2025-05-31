public class MemberRequest
{
    public Guid ProjectId { get; set; }
    public Guid? TeamId { get; set; } = null;
    public Guid UserId { get; set; }
    public Guid? SuperiorId { get; set; }
    public int RoleId { get; set; }
    public bool IsActivated { get; set; } = false;
    public string Status { get; set; }
}
