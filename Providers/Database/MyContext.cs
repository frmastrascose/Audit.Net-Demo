using Audit.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Audit.Demo.Providers.Database
{
    public class MyContext : AuditDbContext
    {
        public MyContext(DbContextOptions<MyContext> options) : base(options)
        {
        }
        public DbSet<ValueEntity> Values { get; set; }
    }
}
