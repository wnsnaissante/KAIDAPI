namespace KaidAPI.Repositories;

public interface IFlagRepository
{
    Task<Guid> CreateFlagAsync(Flag flag);
    Task<Flag> GetFlagByFlagIdAsync(Guid flagId);
    Task UpdateFlagAsync(Flag flag);
    Task DeleteFlagAsync(Guid flagId);
}