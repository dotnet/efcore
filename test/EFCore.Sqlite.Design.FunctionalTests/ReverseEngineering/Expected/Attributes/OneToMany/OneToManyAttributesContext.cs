using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2E.Sqlite
{
    public partial class OneToManyAttributesContext : DbContext
    {
        public virtual DbSet<OneToManyDependent> OneToManyDependent { get; set; }
        public virtual DbSet<OneToManyPrincipal> OneToManyPrincipal { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlite(@"Data Source=OneToManyAttributes.db;Cache=Private");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OneToManyDependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToManyDependentId1, e.OneToManyDependentId2 })
                    .HasName("sqlite_autoindex_OneToManyDependent_1");
            });

            modelBuilder.Entity<OneToManyPrincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToManyPrincipalId1, e.OneToManyPrincipalId2 })
                    .HasName("sqlite_autoindex_OneToManyPrincipal_1");
            });
        }
    }
}