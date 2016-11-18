using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2E.Sqlite
{
    public partial class SelfRefFluentApiContext : DbContext
    {
        public virtual DbSet<SelfRef> SelfRef { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlite(@"Data Source=SelfRefFluentApi.db;Cache=Private");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<SelfRef>(entity =>
            {
                entity.HasOne(d => d.SelfForeignKeyNavigation)
                    .WithMany(p => p.InverseSelfForeignKeyNavigation)
                    .HasForeignKey(d => d.SelfForeignKey);
            });
        }
    }
}