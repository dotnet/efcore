using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2E.Sqlite
{
    public partial class OneToOneAttributesContext : DbContext
    {
        public virtual DbSet<Dependent> Dependent { get; set; }
        public virtual DbSet<Principal> Principal { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlite(@"Data Source=OneToOneAttributes.db;Cache=Private");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Dependent>(entity =>
            {
                entity.HasIndex(e => e.PrincipalId)
                    .HasName("sqlite_autoindex_Dependent_1")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.Principal)
                    .WithOne(p => p.Dependent)
                    .HasForeignKey<Dependent>(d => d.PrincipalId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<Principal>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();
            });
        }
    }
}