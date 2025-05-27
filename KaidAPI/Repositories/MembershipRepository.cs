using KaidAPI.Context;
using KaidAPI.Models;
using KaidAPI.Repositories;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Membership>> GetActivatedMembershipsByUserIdAsync(Guid userId)
    {
        var memberships = await _context.Memberships.Where(x => x.UserId == userId && x.IsActivated == true).ToListAsync();
        return memberships;
    }

    public async Task<List<Membership>> GetDeactivatedMembershipsByUserIdAsync(Guid userId)
    {
        var memberships = await _context.Memberships.Where(x => x.UserId == userId && x.IsActivated == false).ToListAsync();
        return memberships;
    }


    public async Task<Membership> GetMembershipByProjectIdAndUserIdAsync(Guid projectId, Guid userId)
    {
        var membership = await _context.Memberships.Where(x => x.ProjectId == projectId && x.UserId == userId).FirstOrDefaultAsync();
        return membership;
    }

    public async Task<Membership> GetMembershipByMembershipIdAsync(Guid membershipId) {
        var membership = await _context.Memberships.FindAsync(membershipId);
        return membership;
    }

    public async Task<OperationResult> DeleteMembershipAsync(Guid membershipId) {
        var membership = await _context.Memberships.FindAsync(membershipId);
        if (membership == null) {
            return new OperationResult
            {
                Success = false,
                Message = "Membership not found"
            };
        }

        _context.Memberships.Remove(membership);
        await _context.SaveChangesAsync();
        return new OperationResult
        {
            Success = true,
            Message = "Membership deleted successfully"
        };
    }

    public async Task<IEnumerable<Membership>> GetAllMembershipsAsync()
    {
        return await _context.Memberships.ToListAsync();
    }
}
