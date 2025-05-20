using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface IMembershipRepository {
    Task<OperationResult> CreateMembershipAsync(Membership membership);
}
