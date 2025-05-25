public class MemberRequest
{
    public Guid ProjectId { get; set; }
    public Guid UserId { get; set; }
    public Guid? SuperiorId { get; set; }
    public int RoleId { get; set; }
    public bool isActivated { get; set; }
}
