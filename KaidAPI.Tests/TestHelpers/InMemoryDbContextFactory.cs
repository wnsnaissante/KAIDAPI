using Microsoft.EntityFrameworkCore;
using KaidAPI.Context;

namespace KaidAPI.Tests.TestHelpers
{
    public static class InMemoryDbContextFactory
    {
        public static ServerDbContext Create(string dbName = null)
        {
            var options = new DbContextOptionsBuilder<ServerDbContext>()
                .UseInMemoryDatabase(databaseName: dbName ?? Guid.NewGuid().ToString())
                .Options;

            return new ServerDbContext(options);
        }
    }
}