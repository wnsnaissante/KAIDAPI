using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface IMembershipRepository {
    Task<OperationResult> CreateMembershipAsync(Membership membership);
    Task<List<Membership>> GetMembershipsByUserIdAsync(Guid userId);
    Task<Membership> GetMembershipByProjectIdAndUserIdAsync(Guid projectId, Guid userId);
}
