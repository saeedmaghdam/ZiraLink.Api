﻿using Microsoft.EntityFrameworkCore;
using ZiraLink.Domain;

namespace ZiraLink.Api.Application
{
    public class AppDbContext : DbContext
    {
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<AppProject> AppProjects { get; set; }

        public string DbPath { get; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        //public AppDbContext() { }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlite(@"Data Source=database.db");
        //}
    }
}
