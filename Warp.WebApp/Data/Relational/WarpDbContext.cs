using Microsoft.EntityFrameworkCore;

namespace Warp.WebApp.Data.Relational;

public class WarpDbContext : DbContext
{
    public WarpDbContext(DbContextOptions<WarpDbContext> options) : base(options)
    {
    }
}
