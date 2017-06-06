using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace E2ETest.Namespace
{
    public partial class SqlServerReverseEngineerTestE2EContext : DbContext
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
                entity.Property(e => e.AllDataTypesId).HasColumnName("AllDataTypesID");

                entity.Property(e => e.BigintColumn).HasColumnName("bigintColumn");

                entity.Property(e => e.Binary111Column)
                    .HasColumnName("binary111Column")
                    .HasColumnType("binary(111)");

                entity.Property(e => e.BinaryColumn)
                    .HasColumnName("binaryColumn")
                    .HasColumnType("binary(1)");

                entity.Property(e => e.BinaryVarying133Column)
                    .HasColumnName("binaryVarying133Column")
                    .HasMaxLength(133);

                entity.Property(e => e.BinaryVaryingColumn)
                    .HasColumnName("binaryVaryingColumn")
                    .HasMaxLength(1);

                entity.Property(e => e.BinaryVaryingMaxColumn).HasColumnName("binaryVaryingMaxColumn");

                entity.Property(e => e.BitColumn).HasColumnName("bitColumn");

                entity.Property(e => e.Char10Column)
                    .HasColumnName("char10Column")
                    .HasColumnType("char(10)");

                entity.Property(e => e.CharColumn)
                    .HasColumnName("charColumn")
                    .HasColumnType("char(1)");

                entity.Property(e => e.CharVarying144Column)
                    .HasColumnName("charVarying144Column")
                    .HasMaxLength(144)
                    .IsUnicode(false);

                entity.Property(e => e.CharVaryingColumn)
                    .HasColumnName("charVaryingColumn")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.CharVaryingMaxColumn)
                    .HasColumnName("charVaryingMaxColumn")
                    .IsUnicode(false);

                entity.Property(e => e.Character155Column)
                    .HasColumnName("character155Column")
                    .HasColumnType("char(155)");

                entity.Property(e => e.CharacterColumn)
                    .HasColumnName("characterColumn")
                    .HasColumnType("char(1)");

                entity.Property(e => e.CharacterVarying166Column)
                    .HasColumnName("characterVarying166Column")
                    .HasMaxLength(166)
                    .IsUnicode(false);

                entity.Property(e => e.CharacterVaryingColumn)
                    .HasColumnName("characterVaryingColumn")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.CharacterVaryingMaxColumn)
                    .HasColumnName("characterVaryingMaxColumn")
                    .IsUnicode(false);

                entity.Property(e => e.DateColumn)
                    .HasColumnName("dateColumn")
                    .HasColumnType("date");

                entity.Property(e => e.Datetime24Column)
                    .HasColumnName("datetime24Column")
                    .HasColumnType("datetime2(4)");

                entity.Property(e => e.Datetime2Column).HasColumnName("datetime2Column");

                entity.Property(e => e.DatetimeColumn)
                    .HasColumnName("datetimeColumn")
                    .HasColumnType("datetime");

                entity.Property(e => e.Datetimeoffset5Column)
                    .HasColumnName("datetimeoffset5Column")
                    .HasColumnType("datetimeoffset(5)");

                entity.Property(e => e.DatetimeoffsetColumn).HasColumnName("datetimeoffsetColumn");

                entity.Property(e => e.Decimal105Column)
                    .HasColumnName("decimal105Column")
                    .HasColumnType("decimal(10, 5)");

                entity.Property(e => e.DecimalColumn)
                    .HasColumnName("decimalColumn")
                    .HasColumnType("decimal(18, 0)");

                entity.Property(e => e.DecimalDefaultColumn).HasColumnName("decimalDefaultColumn");

                entity.Property(e => e.FloatColumn).HasColumnName("floatColumn");

                entity.Property(e => e.ImageColumn)
                    .HasColumnName("imageColumn")
                    .HasColumnType("image");

                entity.Property(e => e.IntColumn).HasColumnName("intColumn");

                entity.Property(e => e.MoneyColumn)
                    .HasColumnName("moneyColumn")
                    .HasColumnType("money");

                entity.Property(e => e.NationalCharVarying177Column)
                    .HasColumnName("nationalCharVarying177Column")
                    .HasMaxLength(177);

                entity.Property(e => e.NationalCharVaryingColumn)
                    .HasColumnName("nationalCharVaryingColumn")
                    .HasMaxLength(1);

                entity.Property(e => e.NationalCharVaryingMaxColumn).HasColumnName("nationalCharVaryingMaxColumn");

                entity.Property(e => e.NationalCharacter171Column)
                    .HasColumnName("nationalCharacter171Column")
                    .HasColumnType("nchar(171)");

                entity.Property(e => e.NationalCharacterColumn)
                    .HasColumnName("nationalCharacterColumn")
                    .HasColumnType("nchar(1)");

                entity.Property(e => e.NationalCharacterVarying188Column)
                    .HasColumnName("nationalCharacterVarying188Column")
                    .HasMaxLength(188);

                entity.Property(e => e.NationalCharacterVaryingColumn)
                    .HasColumnName("nationalCharacterVaryingColumn")
                    .HasMaxLength(1);

                entity.Property(e => e.NationalCharacterVaryingMaxColumn).HasColumnName("nationalCharacterVaryingMaxColumn");

                entity.Property(e => e.Nchar99Column)
                    .HasColumnName("nchar99Column")
                    .HasColumnType("nchar(99)");

                entity.Property(e => e.NcharColumn)
                    .HasColumnName("ncharColumn")
                    .HasColumnType("nchar(1)");

                entity.Property(e => e.NtextColumn)
                    .HasColumnName("ntextColumn")
                    .HasColumnType("ntext");

                entity.Property(e => e.Numeric152Column)
                    .HasColumnName("numeric152Column")
                    .HasColumnType("numeric(15, 2)");

                entity.Property(e => e.NumericColumn)
                    .HasColumnName("numericColumn")
                    .HasColumnType("numeric(18, 0)");

                entity.Property(e => e.NumericDefaultColumn)
                    .HasColumnName("numericDefaultColumn")
                    .HasColumnType("numeric(18, 2)");

                entity.Property(e => e.Nvarchar100Column)
                    .HasColumnName("nvarchar100Column")
                    .HasMaxLength(100);

                entity.Property(e => e.NvarcharColumn)
                    .HasColumnName("nvarcharColumn")
                    .HasMaxLength(1);

                entity.Property(e => e.NvarcharMaxColumn).HasColumnName("nvarcharMaxColumn");

                entity.Property(e => e.RealColumn).HasColumnName("realColumn");

                entity.Property(e => e.SmalldatetimeColumn)
                    .HasColumnName("smalldatetimeColumn")
                    .HasColumnType("smalldatetime");

                entity.Property(e => e.SmallintColumn).HasColumnName("smallintColumn");

                entity.Property(e => e.SmallmoneyColumn)
                    .HasColumnName("smallmoneyColumn")
                    .HasColumnType("smallmoney");

                entity.Property(e => e.TextColumn)
                    .HasColumnName("textColumn")
                    .HasColumnType("text");

                entity.Property(e => e.Time4Column)
                    .HasColumnName("time4Column")
                    .HasColumnType("time(4)");

                entity.Property(e => e.TimeColumn).HasColumnName("timeColumn");

                entity.Property(e => e.TimestampColumn)
                    .HasColumnName("timestampColumn")
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.TinyintColumn).HasColumnName("tinyintColumn");

                entity.Property(e => e.TypeAliasColumn)
                    .HasColumnName("typeAliasColumn")
                    .HasColumnType("TestTypeAlias");

                entity.Property(e => e.UniqueidentifierColumn).HasColumnName("uniqueidentifierColumn");

                entity.Property(e => e.Varbinary123Column)
                    .HasColumnName("varbinary123Column")
                    .HasMaxLength(123);

                entity.Property(e => e.VarbinaryColumn)
                    .HasColumnName("varbinaryColumn")
                    .HasMaxLength(1);

                entity.Property(e => e.VarbinaryMaxColumn).HasColumnName("varbinaryMaxColumn");

                entity.Property(e => e.Varchar66Column)
                    .HasColumnName("varchar66Column")
                    .HasMaxLength(66)
                    .IsUnicode(false);

                entity.Property(e => e.VarcharColumn)
                    .HasColumnName("varcharColumn")
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.VarcharMaxColumn)
                    .HasColumnName("varcharMaxColumn")
                    .IsUnicode(false);

                entity.Property(e => e.XmlColumn)
                    .HasColumnName("xmlColumn")
                    .HasColumnType("xml");
            });

            modelBuilder.Entity<MultipleFksDependent>(entity =>
            {
                entity.ToTable("MultipleFKsDependent");

                entity.Property(e => e.MultipleFksDependentId)
                    .HasColumnName("MultipleFKsDependentId")
                    .ValueGeneratedNever();

                entity.Property(e => e.AnotherColumn)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.RelationAid).HasColumnName("RelationAId");

                entity.Property(e => e.RelationBid).HasColumnName("RelationBId");

                entity.Property(e => e.RelationCid).HasColumnName("RelationCId");

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
                entity.ToTable("MultipleFKsPrincipal");

                entity.Property(e => e.MultipleFksPrincipalId)
                    .HasColumnName("MultipleFKsPrincipalId")
                    .ValueGeneratedNever();

                entity.Property(e => e.SomePrincipalColumn)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<OneToManyDependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToManyDependentId1, e.OneToManyDependentId2 });

                entity.Property(e => e.OneToManyDependentId1).HasColumnName("OneToManyDependentID1");

                entity.Property(e => e.OneToManyDependentId2).HasColumnName("OneToManyDependentID2");

                entity.Property(e => e.OneToManyDependentFk1).HasColumnName("OneToManyDependentFK1");

                entity.Property(e => e.OneToManyDependentFk2).HasColumnName("OneToManyDependentFK2");

                entity.Property(e => e.SomeDependentEndColumn)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(d => d.OneToManyDependentFk)
                    .WithMany(p => p.OneToManyDependent)
                    .HasForeignKey(d => new { d.OneToManyDependentFk1, d.OneToManyDependentFk2 })
                    .HasConstraintName("FK_OneToManyDependent");
            });

            modelBuilder.Entity<OneToManyPrincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToManyPrincipalId1, e.OneToManyPrincipalId2 });

                entity.Property(e => e.OneToManyPrincipalId1).HasColumnName("OneToManyPrincipalID1");

                entity.Property(e => e.OneToManyPrincipalId2).HasColumnName("OneToManyPrincipalID2");

                entity.Property(e => e.Other)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<OneToOneDependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneDependentId1, e.OneToOneDependentId2 });

                entity.Property(e => e.OneToOneDependentId1).HasColumnName("OneToOneDependentID1");

                entity.Property(e => e.OneToOneDependentId2).HasColumnName("OneToOneDependentID2");

                entity.Property(e => e.SomeDependentEndColumn)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(d => d.OneToOneDependentNavigation)
                    .WithOne(p => p.OneToOneDependent)
                    .HasForeignKey<OneToOneDependent>(d => new { d.OneToOneDependentId1, d.OneToOneDependentId2 })
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_OneToOneDependent");
            });

            modelBuilder.Entity<OneToOneFktoUniqueKeyDependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneFktoUniqueKeyDependentId1, e.OneToOneFktoUniqueKeyDependentId2 });

                entity.ToTable("OneToOneFKToUniqueKeyDependent");

                entity.HasIndex(e => new { e.OneToOneFktoUniqueKeyDependentFk1, e.OneToOneFktoUniqueKeyDependentFk2 })
                    .HasName("UK_OneToOneFKToUniqueKeyDependent")
                    .IsUnique()
                    .ForSqlServerIsClustered();

                entity.Property(e => e.OneToOneFktoUniqueKeyDependentId1).HasColumnName("OneToOneFKToUniqueKeyDependentID1");

                entity.Property(e => e.OneToOneFktoUniqueKeyDependentId2).HasColumnName("OneToOneFKToUniqueKeyDependentID2");

                entity.Property(e => e.OneToOneFktoUniqueKeyDependentFk1).HasColumnName("OneToOneFKToUniqueKeyDependentFK1");

                entity.Property(e => e.OneToOneFktoUniqueKeyDependentFk2).HasColumnName("OneToOneFKToUniqueKeyDependentFK2");

                entity.Property(e => e.SomeColumn)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(d => d.OneToOneFktoUniqueKeyDependentFk)
                    .WithOne(p => p.OneToOneFktoUniqueKeyDependent)
                    .HasPrincipalKey<OneToOneFktoUniqueKeyPrincipal>(p => new { p.OneToOneFktoUniqueKeyPrincipalUniqueKey1, p.OneToOneFktoUniqueKeyPrincipalUniqueKey2 })
                    .HasForeignKey<OneToOneFktoUniqueKeyDependent>(d => new { d.OneToOneFktoUniqueKeyDependentFk1, d.OneToOneFktoUniqueKeyDependentFk2 })
                    .HasConstraintName("FK_OneToOneFKToUniqueKeyDependent");
            });

            modelBuilder.Entity<OneToOneFktoUniqueKeyPrincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneFktoUniqueKeyPrincipalId1, e.OneToOneFktoUniqueKeyPrincipalId2 });

                entity.ToTable("OneToOneFKToUniqueKeyPrincipal");

                entity.HasIndex(e => new { e.OneToOneFktoUniqueKeyPrincipalUniqueKey1, e.OneToOneFktoUniqueKeyPrincipalUniqueKey2 })
                    .HasName("UK_OneToOneFKToUniqueKeyPrincipal")
                    .IsUnique();

                entity.Property(e => e.OneToOneFktoUniqueKeyPrincipalId1).HasColumnName("OneToOneFKToUniqueKeyPrincipalID1");

                entity.Property(e => e.OneToOneFktoUniqueKeyPrincipalId2).HasColumnName("OneToOneFKToUniqueKeyPrincipalID2");

                entity.Property(e => e.OneToOneFktoUniqueKeyPrincipalUniqueKey1).HasColumnName("OneToOneFKToUniqueKeyPrincipalUniqueKey1");

                entity.Property(e => e.OneToOneFktoUniqueKeyPrincipalUniqueKey2).HasColumnName("OneToOneFKToUniqueKeyPrincipalUniqueKey2");

                entity.Property(e => e.SomePrincipalColumn)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<OneToOnePrincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToOnePrincipalId1, e.OneToOnePrincipalId2 });

                entity.Property(e => e.OneToOnePrincipalId1).HasColumnName("OneToOnePrincipalID1");

                entity.Property(e => e.OneToOnePrincipalId2).HasColumnName("OneToOnePrincipalID2");

                entity.Property(e => e.SomeOneToOnePrincipalColumn)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<OneToOneSeparateFkdependent>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneSeparateFkdependentId1, e.OneToOneSeparateFkdependentId2 });

                entity.ToTable("OneToOneSeparateFKDependent");

                entity.HasIndex(e => new { e.OneToOneSeparateFkdependentFk1, e.OneToOneSeparateFkdependentFk2 })
                    .HasName("UK_OneToOneSeparateFKDependent")
                    .IsUnique();

                entity.Property(e => e.OneToOneSeparateFkdependentId1).HasColumnName("OneToOneSeparateFKDependentID1");

                entity.Property(e => e.OneToOneSeparateFkdependentId2).HasColumnName("OneToOneSeparateFKDependentID2");

                entity.Property(e => e.OneToOneSeparateFkdependentFk1).HasColumnName("OneToOneSeparateFKDependentFK1");

                entity.Property(e => e.OneToOneSeparateFkdependentFk2).HasColumnName("OneToOneSeparateFKDependentFK2");

                entity.Property(e => e.SomeDependentEndColumn)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasOne(d => d.OneToOneSeparateFkdependentFk)
                    .WithOne(p => p.OneToOneSeparateFkdependent)
                    .HasForeignKey<OneToOneSeparateFkdependent>(d => new { d.OneToOneSeparateFkdependentFk1, d.OneToOneSeparateFkdependentFk2 })
                    .HasConstraintName("FK_OneToOneSeparateFKDependent");
            });

            modelBuilder.Entity<OneToOneSeparateFkprincipal>(entity =>
            {
                entity.HasKey(e => new { e.OneToOneSeparateFkprincipalId1, e.OneToOneSeparateFkprincipalId2 });

                entity.ToTable("OneToOneSeparateFKPrincipal");

                entity.Property(e => e.OneToOneSeparateFkprincipalId1).HasColumnName("OneToOneSeparateFKPrincipalID1");

                entity.Property(e => e.OneToOneSeparateFkprincipalId2).HasColumnName("OneToOneSeparateFKPrincipalID2");

                entity.Property(e => e.SomeOneToOneSeparateFkprincipalColumn)
                    .IsRequired()
                    .HasColumnName("SomeOneToOneSeparateFKPrincipalColumn")
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<PropertyConfiguration>(entity =>
            {
                entity.HasIndex(e => new { e.A, e.B })
                    .HasName("Test_PropertyConfiguration_Index");

                entity.Property(e => e.PropertyConfigurationId)
                    .HasColumnName("PropertyConfigurationID")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.ComputedDateTimeColumn)
                    .HasColumnType("datetime")
                    .HasComputedColumnSql("(getdate())");

                entity.Property(e => e.PropertyConfiguration1).HasColumnName("PropertyConfiguration");

                entity.Property(e => e.RowversionColumn)
                    .IsRequired()
                    .HasColumnType("timestamp")
                    .ValueGeneratedOnAddOrUpdate();

                entity.Property(e => e.SumOfAandB)
                    .HasColumnName("SumOfAAndB")
                    .HasComputedColumnSql("([A]+[B])");

                entity.Property(e => e.WithDateDefaultExpression).HasDefaultValueSql("(getdate())");

                entity.Property(e => e.WithDateFixedDefault).HasDefaultValueSql("('October 20, 2015 11am')");

                entity.Property(e => e.WithDefaultValue).HasDefaultValueSql("((-1))");

                entity.Property(e => e.WithGuidDefaultExpression).HasDefaultValueSql("(newsequentialid())");

                entity.Property(e => e.WithMoneyDefaultValue)
                    .HasColumnType("money")
                    .HasDefaultValueSql("((0.00))");

                entity.Property(e => e.WithVarcharNullDefaultValue)
                    .HasMaxLength(1)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<SelfReferencing>(entity =>
            {
                entity.Property(e => e.SelfReferencingId)
                    .HasColumnName("SelfReferencingID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Description)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.Name)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(e => e.SelfReferenceFk).HasColumnName("SelfReferenceFK");

                entity.HasOne(d => d.SelfReferenceFkNavigation)
                    .WithMany(p => p.InverseSelfReferenceFkNavigation)
                    .HasForeignKey(d => d.SelfReferenceFk)
                    .HasConstraintName("FK_SelfReferencing");
            });

            modelBuilder.Entity<TestSpacesKeywordsTable>(entity =>
            {
                entity.ToTable("Test Spaces Keywords Table");

                entity.Property(e => e.TestSpacesKeywordsTableId)
                    .HasColumnName("Test Spaces Keywords TableID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Abstract).HasColumnName("abstract");

                entity.Property(e => e.AtSymbolAtStartOfColumn).HasColumnName("@AtSymbolAtStartOfColumn");

                entity.Property(e => e.BackslashesInColumn).HasColumnName("\\Backslashes\\In\\Column");

                entity.Property(e => e.Class).HasColumnName("class");

                entity.Property(e => e.CommasInColumn).HasColumnName("Commas,In,Column");

                entity.Property(e => e.DollarSignColumn).HasColumnName("$Dollar$Sign$Column");

                entity.Property(e => e.DoubleQuotesColumn).HasColumnName("\"Double\"Quotes\"Column");

                entity.Property(e => e.ExclamationMarkColumn).HasColumnName("!Exclamation!Mark!Column");

                entity.Property(e => e.MultipleAtSymbolsInColumn).HasColumnName("@Multiple@At@Symbols@In@Column");

                entity.Property(e => e.SpacesInColumn).HasColumnName("Spaces In Column");

                entity.Property(e => e.TabsInColumn).HasColumnName("Tabs\tIn\tColumn");

                entity.Property(e => e.Volatile).HasColumnName("volatile");
            });

            modelBuilder.Entity<UnmappablePkcolumn>(entity =>
            {
                entity.ToTable("UnmappablePKColumn");

                entity.Property(e => e.UnmappablePkcolumnId)
                    .HasColumnName("UnmappablePKColumnID")
                    .ValueGeneratedNever();

                entity.Property(e => e.Acolumn)
                    .IsRequired()
                    .HasColumnName("AColumn")
                    .HasMaxLength(20);

                entity.Property(e => e.ValueGeneratedOnAddColumn).ValueGeneratedOnAdd();
            });
        }
    }
}