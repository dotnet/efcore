using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2E.Sqlite
{
    public partial class OneToManyFluentApiContext : DbContext
    {
        public virtual DbSet<OneToManyDependent> OneToManyDependent { get; set; }
        public virtual DbSet<OneToManyPrincipal> OneToManyPrincipal { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlite(@"Data Source=OneToManyFluentApi.db;Cache=Private");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OneToManyDependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToManyDependentId1, e.OneToManyDependentId2 })
                    .HasName("sqlite_autoindex_OneToManyDependent_1");

                entity.Property(e => e.OneToManyDependentId1)
                    .HasColumnName("OneToManyDependentID1")
                    .HasColumnType("INT");

                entity.Property(e => e.OneToManyDependentId2)
                    .HasColumnName("OneToManyDependentID2")
                    .HasColumnType("INT");

                entity.Property(e => e.OneToManyDependentFk1)
                    .HasColumnName("OneToManyDependentFK1")
                    .HasColumnType("INT");

                entity.Property(e => e.OneToManyDependentFk2)
                    .HasColumnName("OneToManyDependentFK2")
                    .HasColumnType("INT");

                entity.Property(e => e.SomeDependentEndColumn)
                    .IsRequired()
                    .HasColumnType("VARCHAR");

                entity.HasOne(d => d.OneToManyDependentFk)
                    .WithMany(p => p.OneToManyDependent)
                    .HasForeignKey(d => new { d.OneToManyDependentFk1, d.OneToManyDependentFk2 });
            });

            modelBuilder.Entity<OneToManyPrincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToManyPrincipalId1, e.OneToManyPrincipalId2 })
                    .HasName("sqlite_autoindex_OneToManyPrincipal_1");

                entity.Property(e => e.OneToManyPrincipalId1)
                    .HasColumnName("OneToManyPrincipalID1")
                    .HasColumnType("INT");

                entity.Property(e => e.OneToManyPrincipalId2)
                    .HasColumnName("OneToManyPrincipalID2")
                    .HasColumnType("INT");

                entity.Property(e => e.Other).IsRequired();
            });
        }
    }
}