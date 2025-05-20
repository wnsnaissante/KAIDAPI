using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly ServerDbContext _context;

    public MembershipRepository(ServerDbContext context) {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<OperationResult> CreateMembershipAsync(Membership membership) {
        _context.Memberships.Add(membership);
        await _context.SaveChangesAsync();
        return new OperationResult
        {
            Success = true,
            Message = "Team membership created successfully"
        };
    }

}
