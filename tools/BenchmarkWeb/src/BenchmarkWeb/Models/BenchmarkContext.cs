using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BenchmarkWeb.Models
{
    public class BenchmarkContext : DbContext
    {
        public DbSet<Run> Runs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Run>().ToTable("Runs");
        }
    }
}
