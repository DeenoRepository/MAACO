using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MAACO.Persistence.Data;

public sealed class MaacoDbContextFactory : IDesignTimeDbContextFactory<MaacoDbContext>
{
    public MaacoDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MaacoDbContext>();
        optionsBuilder.UseSqlite("Data Source=maaco.db");
        return new MaacoDbContext(optionsBuilder.Options);
    }
}
