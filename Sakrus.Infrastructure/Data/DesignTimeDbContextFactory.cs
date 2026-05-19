using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Sakrus.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Usa variável de ambiente para segurança (ou padrão para desenvolvimento)
        var connectionString = Environment.GetEnvironmentVariable("SAKRUS_DB_CONNECTION") 
            ?? "Host=localhost;Port=5432;Database=sakrus_db;Username=postgres;Password=1234";
        
        optionsBuilder.UseNpgsql(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}