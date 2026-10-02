using Microsoft.EntityFrameworkCore;
using Sakrus.Core;
using Sakrus.Infrastructure.Data;

namespace Sakrus.Tests.Documentos;

public class TestDbContextFactory : IDbContextFactory<ApplicationDbContext>
{
    private readonly string _dbName;

    public TestDbContextFactory(string dbName)
    {
        _dbName = dbName;
    }

    public ApplicationDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(_dbName)
            .Options;

        return new ApplicationDbContext(options);
    }
}

public class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow;
    }

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void SetUtcNow(DateTimeOffset value) => _utcNow = value;
}

public class FakeCurrentUser : ICurrentUserService
{
    public int? UserId => 1;

    public string UserName => "Usuário Teste";
}
