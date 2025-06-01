using KaidAPI.Models;
using KaidAPI.ViewModel;
namespace KaidAPI.Services;

public interface IMembershipService
{
    public Task<OperationResult> CreateMembershipAsync(string oidcSub, MemberRequest memberRequest);
    public Task<OperationResult> DeleteMembershipAsync(string oidcSub, Guid membershipId);
    public Task<OperationResult> GetUserRoleByProjectIdAndUserIdAsync(string oidcSub, Guid projectId);
    public Task<OperationResult> GetMembersAsync(string oidcSub, Guid projectId, Guid teamId);
    public Task<OperationResult> UpdateMembershipAsync(string oidcSub, Guid membershipId, MemberRequest memberRequest);
    public Task<OperationResult> GetMembershipAsync(string oidcSub, Guid projectId, Guid teamId, Guid userId);
    public Task<OperationResult> AcceptInvitationAsync(string oidcSub, Guid membershipId);
    public Task<OperationResult> DenyInvitationAsync(string oidcSub, Guid membershipId);
}