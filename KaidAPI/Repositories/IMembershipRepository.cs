using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface IMembershipRepository {
    Task<OperationResult> CreateMembershipAsync(Membership membership);
    Task<List<Membership>> GetMembershipsByUserIdAsync(Guid userId);
    Task<Membership> GetMembershipByProjectIdAndUserIdAsync(Guid projectId, Guid userId);
    Task<Membership> GetMembershipByMembershipIdAsync(Guid membershipId);
    Task<OperationResult> DeleteMembershipAsync(Guid membershipId);
    Task<IEnumerable<Membership>> GetAllMembershipsAsync();
}
