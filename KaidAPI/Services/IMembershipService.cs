using KaidAPI.Models;
using KaidAPI.ViewModel;
namespace KaidAPI.Services;

public interface IMembershipService
{
    public Task<OperationResult> CreateMembershipAsync(string oidcSub, MemberRequest memberRequest);
    public Task<OperationResult> DeleteMembershipAsync(string oidcSub, Guid membershipId);
    public Task<OperationResult> GetUserRoleByProjectIdAndUserIdAsync(string oidcSub, Guid projectId);
    public Task<IEnumerable<Membership>> GetMembersAsync(Guid projectId, Guid teamId);
}