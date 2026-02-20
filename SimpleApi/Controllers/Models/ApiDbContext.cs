using Microsoft.EntityFrameworkCore;

namespace SimpleApi.Models
{
    public class ApiDbContext : DbContext
    {
        public ApiDbContext(DbContextOptions<ApiDbContext> options) : base(options) { }

        public DbSet<LeaveRequest> LeaveRequests { get; set; } = default!;
        public DbSet<Holiday> Holidays { get; set; } = default!;
        public DbSet<User> Users { get; set; } = default!;
        public DbSet<AuditLog> AuditLogs { get; set; } = default!;
    }
}