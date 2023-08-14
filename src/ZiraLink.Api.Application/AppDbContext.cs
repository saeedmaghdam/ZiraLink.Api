using System.Reflection;
using Microsoft.EntityFrameworkCore;
using ZiraLink.Domain;

namespace ZiraLink.Api.Application
{
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Project> Projects { get; set; }

        public string DbPath { get; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    var pathToExe = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);
        //    optionsBuilder.UseSqlite($"Data Source={System.IO.Path.Join(pathToExe, "database.db")}");
        //}
    }
}
