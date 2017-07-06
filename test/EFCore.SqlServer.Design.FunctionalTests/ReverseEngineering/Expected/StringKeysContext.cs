using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2ETest.Namespace
{
    public partial class StringKeysContext : DbContext
    {
        public virtual DbSet<StringKeysBlogs> StringKeysBlogs { get; set; }
        public virtual DbSet<StringKeysPosts> StringKeysPosts { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. See http://go.microsoft.com/fwlink/?LinkId=723263 for guidance on storing connection strings.
                optionsBuilder.UseSqlServer(@"{{connectionString}}");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<StringKeysBlogs>(entity =>
            {
                entity.HasKey(e => e.PrimaryKey);

                entity.HasIndex(e => e.AlternateKey)
                    .HasName("AK_StringKeysBlogs_AlternateKey")
                    .IsUnique();

                entity.HasIndex(e => e.IndexProperty);

                entity.Property(e => e.PrimaryKey).ValueGeneratedNever();

                entity.Property(e => e.AlternateKey).IsRequired();

                entity.Property(e => e.RowVersion).IsRowVersion();
            });

            modelBuilder.Entity<StringKeysPosts>(entity =>
            {
                entity.HasIndex(e => e.BlogAlternateKey);

                entity.HasOne(d => d.BlogAlternateKeyNavigation)
                    .WithMany(p => p.StringKeysPosts)
                    .HasPrincipalKey(p => p.AlternateKey)
                    .HasForeignKey(d => d.BlogAlternateKey);
            });
        }
    }
}
