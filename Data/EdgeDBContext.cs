using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminPortal.Data;
using AdminPortal.Models.Auth;
using Microsoft.EntityFrameworkCore;

namespace AdminPortal.Models
{
    public class EdgeDBContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<ClientMetric> ClientMetrics { get; set; }
        public DbSet<ClientTask> ClientTasks { get; set; }
        public DbSet<DataItem> DataItems { get; set; }
        public DbSet<ClientLog> ClientLogs { get; set; }
        public DbSet<User> Users { get; set; }
        public string mytestvar;
        public EdgeDBContext(DbContextOptions<EdgeDBContext> options)
        : base(options)
        {
            Database.EnsureCreated();
        }
    }
}
