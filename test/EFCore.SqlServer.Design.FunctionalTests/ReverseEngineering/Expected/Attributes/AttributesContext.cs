using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2ETest.Namespace.SubDir
{
    public partial class AttributesContext : DbContext
    {
        public virtual DbSet<AllDataTypes> AllDataTypes { get; set; }
        public virtual DbSet<MultipleFksDependent> MultipleFksDependent { get; set; }
        public virtual DbSet<MultipleFksPrincipal> MultipleFksPrincipal { get; set; }
        public virtual DbSet<OneToManyDependent> OneToManyDependent { get; set; }
        public virtual DbSet<OneToManyPrincipal> OneToManyPrincipal { get; set; }
        public virtual DbSet<OneToOneDependent> OneToOneDependent { get; set; }
        public virtual DbSet<OneToOneFktoUniqueKeyDependent> OneToOneFktoUniqueKeyDependent { get; set; }
        public virtual DbSet<OneToOneFktoUniqueKeyPrincipal> OneToOneFktoUniqueKeyPrincipal { get; set; }
        public virtual DbSet<OneToOnePrincipal> OneToOnePrincipal { get; set; }
        public virtual DbSet<OneToOneSeparateFkdependent> OneToOneSeparateFkdependent { get; set; }
        public virtual DbSet<OneToOneSeparateFkprincipal> OneToOneSeparateFkprincipal { get; set; }
        public virtual DbSet<PropertyConfiguration> PropertyConfiguration { get; set; }
        public virtual DbSet<SelfReferencing> SelfReferencing { get; set; }
        public virtual DbSet<TestSpacesKeywordsTable> TestSpacesKeywordsTable { get; set; }
        public virtual DbSet<UnmappablePkcolumn> UnmappablePkcolumn { get; set; }

        // Unable to generate entity type for table 'dbo.TableWithUnmappablePrimaryKeyColumn'. Please see the warning messages.

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
            modelBuilder.Entity<AllDataTypes>(entity =>
            {
                entity.Property(e => e.CharVarying144Column).IsUnicode(false);

                entity.Property(e => e.CharVaryingColumn).IsUnicode(false);

                entity.Property(e => e.CharVaryingMaxColumn).IsUnicode(false);

                entity.Property(e => e.CharacterVarying166Column).IsUnicode(false);

                entity.Property(e => e.CharacterVaryingColumn).IsUnicode(false);

                entity.Property(e => e.CharacterVaryingMaxColumn).IsUnicode(false);

                entity.Property(e => e.TimestampColumn).ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.Varchar66Column).IsUnicode(false);

                entity.Property(e => e.VarcharColumn).IsUnicode(false);

                entity.Property(e => e.VarcharMaxColumn).IsUnicode(false);
            });

            modelBuilder.Entity<MultipleFksDependent>(entity =>
            {
                entity.Property(e => e.MultipleFksDependentId).ValueGeneratedNever();

                entity.HasOne(d => d.RelationA)
                    .WithMany(p => p.MultipleFksDependentRelationA)
                    .HasForeignKey(d => d.RelationAid)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_RelationA");

                entity.HasOne(d => d.RelationB)
                    .WithMany(p => p.MultipleFksDependentRelationB)
                    .HasForeignKey(d => d.RelationBid)
                    .HasConstraintName("FK_RelationB");

                entity.HasOne(d => d.RelationC)
                    .WithMany(p => p.MultipleFksDependentRelationC)
                    .HasForeignKey(d => d.RelationCid)
                    .HasConstraintName("FK_RelationC");
            });

            modelBuilder.Entity<MultipleFksPrincipal>(entity =>
            {
                entity.Property(e => e.MultipleFksPrincipalId).ValueGeneratedNever();
            });

            modelBuilder.Entity<OneToManyDependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToManyDependentId1, e.OneToManyDependentId2 });

                entity.HasOne(d => d.OneToManyDependentFk)
                    .WithMany(p => p.OneToManyDependent)
                    .HasForeignKey(d => new { d.OneToManyDependentFk1, d.OneToManyDependentFk2 })
                    .HasConstraintName("FK_OneToManyDependent");
            });

            modelBuilder.Entity<OneToManyPrincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToManyPrincipalId1, e.OneToManyPrincipalId2 });
            });

            modelBuilder.Entity<OneToOneDependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneDependentId1, e.OneToOneDependentId2 });

                entity.HasOne(d => d.OneToOneDependentNavigation)
                    .WithOne(p => p.OneToOneDependent)
                    .HasForeignKey<OneToOneDependent>(d => new { d.OneToOneDependentId1, d.OneToOneDependentId2 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OneToOneDependent");
            });

            modelBuilder.Entity<OneToOneFktoUniqueKeyDependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneFktoUniqueKeyDependentId1, e.OneToOneFktoUniqueKeyDependentId2 });

                entity.HasIndex(e => new { e.OneToOneFktoUniqueKeyDependentFk1, e.OneToOneFktoUniqueKeyDependentFk2 })
                    .HasName("UK_OneToOneFKToUniqueKeyDependent")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.HasOne(d => d.OneToOneFktoUniqueKeyDependentFk)
                    .WithOne(p => p.OneToOneFktoUniqueKeyDependent)
                    .HasPrincipalKey<OneToOneFktoUniqueKeyPrincipal>(p => new { p.OneToOneFktoUniqueKeyPrincipalUniqueKey1, p.OneToOneFktoUniqueKeyPrincipalUniqueKey2 })
                    .HasForeignKey<OneToOneFktoUniqueKeyDependent>(d => new { d.OneToOneFktoUniqueKeyDependentFk1, d.OneToOneFktoUniqueKeyDependentFk2 })
                    .HasConstraintName("FK_OneToOneFKToUniqueKeyDependent");
            });

            modelBuilder.Entity<OneToOneFktoUniqueKeyPrincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneFktoUniqueKeyPrincipalId1, e.OneToOneFktoUniqueKeyPrincipalId2 });

                entity.HasIndex(e => new { e.OneToOneFktoUniqueKeyPrincipalUniqueKey1, e.OneToOneFktoUniqueKeyPrincipalUniqueKey2 })
                    .HasName("UK_OneToOneFKToUniqueKeyPrincipal")
                    .IsUnique();
            });

            modelBuilder.Entity<OneToOnePrincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToOnePrincipalId1, e.OneToOnePrincipalId2 });
            });

            modelBuilder.Entity<OneToOneSeparateFkdependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneSeparateFkdependentId1, e.OneToOneSeparateFkdependentId2 });

                entity.HasIndex(e => new { e.OneToOneSeparateFkdependentFk1, e.OneToOneSeparateFkdependentFk2 })
                    .HasName("UK_OneToOneSeparateFKDependent")
                    .IsUnique();

                entity.HasOne(d => d.OneToOneSeparateFkdependentFk)
                    .WithOne(p => p.OneToOneSeparateFkdependent)
                    .HasForeignKey<OneToOneSeparateFkdependent>(d => new { d.OneToOneSeparateFkdependentFk1, d.OneToOneSeparateFkdependentFk2 })
                    .HasConstraintName("FK_OneToOneSeparateFKDependent");
            });

            modelBuilder.Entity<OneToOneSeparateFkprincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneSeparateFkprincipalId1, e.OneToOneSeparateFkprincipalId2 });
            });

            modelBuilder.Entity<PropertyConfiguration>(entity =>
            {
                entity.HasIndex(e => new { e.A, e.B })
                    .HasName("Test_PropertyConfiguration_Index");

                entity.Property(e => e.PropertyConfigurationId).ValueGeneratedOnAdd();

                entity.Property(e => e.ComputedDateTimeColumn).HasComputedColumnSql("(getdate())");

                entity.Property(e => e.RowversionColumn).ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.SumOfAandB).HasComputedColumnSql("([A]+[B])");

                entity.Property(e => e.WithDateDefaultExpression).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.WithDateFixedDefault).HasDefaultValueSql("('October 20, 2015 11am')");

                entity.Property(e => e.WithDefaultValue).HasDefaultValueSql("((-1))");

                entity.Property(e => e.WithGuidDefaultExpression).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.WithMoneyDefaultValue).HasDefaultValueSql("((0.00))");

                entity.Property(e => e.WithVarcharNullDefaultValue).IsUnicode(false);
            });

            modelBuilder.Entity<SelfReferencing>(entity =>
            {
                entity.Property(e => e.SelfReferencingId).ValueGeneratedNever();

                entity.HasOne(d => d.SelfReferenceFkNavigation)
                    .WithMany(p => p.InverseSelfReferenceFkNavigation)
                    .HasForeignKey(d => d.SelfReferenceFk)
                    .HasConstraintName("FK_SelfReferencing");
            });

            modelBuilder.Entity<TestSpacesKeywordsTable>(entity =>
            {
                entity.Property(e => e.TestSpacesKeywordsTableId).ValueGeneratedNever();
            });

            modelBuilder.Entity<UnmappablePkcolumn>(entity =>
            {
                entity.Property(e => e.UnmappablePkcolumnId).ValueGeneratedNever();

                entity.Property(e => e.ValueGeneratedOnAddColumn).ValueGeneratedOnAdd();
            });
        }
    }
}