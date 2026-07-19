using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Clarity01.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // Expose connection to Dapper
        public IDbConnection Connection => Database.GetDbConnection();
    }
}
