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

    public async Task<List<Membership>> GetMembershipsByUserIdAsync(Guid userId)
    {
        var memberships = await _context.Memberships.Where(x => x.UserId == userId).ToListAsync();
        return memberships;
    }

    public async Task<List<Membership>> GetMembershipsByProjectIdAsync(Guid projectId)
    {
        var memberships = await _context.Memberships.Where(x => x.ProjectId == projectId).ToListAsync();
        return memberships;
    }
}
