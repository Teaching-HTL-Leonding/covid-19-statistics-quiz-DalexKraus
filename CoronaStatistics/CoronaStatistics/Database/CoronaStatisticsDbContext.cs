using System;
using CoronaStatistics.Model;
using Microsoft.EntityFrameworkCore;

namespace CoronaStatistics.Database
{
    public class CoronaStatisticsDbContext : DbContext
    {
        public DbSet<FederalState> FederalStates { get; set; }
        public DbSet<District> Districts { get; set; }
        public DbSet<CovidCases> CovidCases { get; set; }

        public CoronaStatisticsDbContext(DbContextOptions options) : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            
        }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.LogTo(Console.WriteLine);
    }
}