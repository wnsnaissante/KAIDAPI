using KaidAPI.Models;

namespace KaidAPI.Repositories;

public interface ICommentRepository
{
    public Task<Guid> CreateCommentAsync(Comment comment);
}