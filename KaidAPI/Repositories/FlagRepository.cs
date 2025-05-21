namespace KaidAPI.Repositories;
public class FlagRepository : IFlagRepository
{
    private readonly ServerDbContext _context;

    public FlagRepository(ServerDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task<Guid> CreateFlagAsync(Flag flag) {
        _context.Flags.Add(flag);
        await _context.SaveChangesAsync();
        return flag.FlagId;
    }

    public async Task<Flag> GetFlagByFlagIdAsync(Guid flagId) {
        return await _context.Flags.FindAsync(flagId);
    }

    public async Task UpdateFlagAsync(Flag flag) {
        _context.Flags.Update(flag);
        await _context.SaveChangesAsync();
    }
    
    public async Task DeleteFlagAsync(Guid flagId) {
        var flag = await GetFlagByFlagIdAsync(flagId);
        if (flag != null) {
            _context.Flags.Remove(flag);
            await _context.SaveChangesAsync();
        }
    } 
}

