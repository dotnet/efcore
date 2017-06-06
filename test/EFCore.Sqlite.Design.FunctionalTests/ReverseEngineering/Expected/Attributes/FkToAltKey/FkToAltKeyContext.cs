using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2E.Sqlite
{
    public partial class FkToAltKeyContext : DbContext
    {
        public virtual DbSet<Comment> Comment { get; set; }
        public virtual DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                #warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlite(@"Data Source=FkToAltKeyAttributes.db;Cache=Private");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.Property(e => e.Id).ValueGeneratedNever();

                entity.HasOne(d => d.UserAlt)
                    .WithMany(p => p.Comment)
                    .HasPrincipalKey(p => p.AltId)
                    .HasForeignKey(d => d.UserAltId)
                    .OnDelete(DeleteBehavior.ClientSetNull);
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasIndex(e => e.AltId)
                    .HasName("sqlite_autoindex_User_1")
                    .IsUnique();

                entity.Property(e => e.Id).ValueGeneratedNever();
            });
        }
    }
}