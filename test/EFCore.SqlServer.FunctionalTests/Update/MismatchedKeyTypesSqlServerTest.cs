// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Update;

public class MismatchedKeyTypesSqlServerTest(MismatchedKeyTypesSqlServerTest.MismatchedKeyTypesSqlServerFixture fixture)
    : IClassFixture<MismatchedKeyTypesSqlServerTest.MismatchedKeyTypesSqlServerFixture>
{
    public MismatchedKeyTypesSqlServerFixture Fixture { get; } = fixture;

    [ConditionalFact] // Issue #28392
    public virtual void Can_update_and_delete_with_bigint_FK_and_int_PK()
    {
        using var context = new MismatchedKeyTypesContext(Fixture);
        context.Database.BeginTransaction();

        var principalEmpty = LoadAndValidateEmpty();
        var principalPopulated = LoadAndValidatePopulated(2);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());

        principalEmpty.OptionalSingle = new OptionalSingleIntLong();
        principalEmpty.RequiredSingle = new RequiredSingleIntLong();
        principalEmpty.OptionalMany.Add(new OptionalManyIntLong());
        principalEmpty.RequiredMany.Add(new RequiredManyIntLong());

        principalPopulated.OptionalSingle = null;
        principalPopulated.RequiredSingle = null;
        principalPopulated.OptionalMany.Clear();
        principalPopulated.RequiredMany.Clear();

        Assert.Equal(12, context.ChangeTracker.Entries().Count());

        context.SaveChanges();

        Assert.Equal(9, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.Clear();

        LoadAndValidateEmpty();
        LoadAndValidatePopulated(1);

        Assert.Equal(6, context.ChangeTracker.Entries().Count());

        Assert.Equal(2, context.Set<OptionalSingleIntLong>().ToList().Count);
        Assert.Equal(1, context.Set<RequiredSingleIntLong>().ToList().Count);
        Assert.Equal(3, context.Set<OptionalManyIntLong>().ToList().Count);
        Assert.Equal(1, context.Set<RequiredManyIntLong>().ToList().Count);

        Assert.Equal(9, context.ChangeTracker.Entries().Count());

        PrincipalIntLong LoadAndValidateEmpty()
        {
            var loaded = Load().Single(e => e.OptionalSingle == null);

            Assert.Null(loaded.OptionalSingle);
            Assert.Null(loaded.RequiredSingle);
            Assert.Empty(loaded.OptionalMany);
            Assert.Empty(loaded.RequiredMany);

            return loaded;
        }

        PrincipalIntLong LoadAndValidatePopulated(int expected)
        {
            var loaded = Load().Single(e => e.OptionalSingle != null);

            Assert.NotNull(loaded.OptionalSingle);
            Assert.NotNull(loaded.RequiredSingle);
            Assert.Equal(expected, loaded.OptionalMany.Count);
            Assert.Equal(expected, loaded.RequiredMany.Count);

            return loaded;
        }

        IQueryable<PrincipalIntLong> Load()
            => context.IntLongs
                .Include(e => e.OptionalMany)
                .Include(e => e.OptionalSingle)
                .Include(e => e.RequiredMany)
                .Include(e => e.RequiredSingle);
    }

    [ConditionalFact] // Issue #28392
    public virtual void Can_update_and_delete_with_tinyint_FK_and_smallint_PK()
    {
        using var context = new MismatchedKeyTypesContext(Fixture);
        context.Database.BeginTransaction();

        var principalEmpty = LoadAndValidateEmpty();
        var principalPopulated = LoadAndValidatePopulated(2);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());

        principalEmpty.OptionalSingle = new OptionalSingleShortByte();
        principalEmpty.RequiredSingle = new RequiredSingleShortByte();
        principalEmpty.OptionalMany.Add(new OptionalManyShortByte());
        principalEmpty.RequiredMany.Add(new RequiredManyShortByte());

        principalPopulated.OptionalSingle = null;
        principalPopulated.RequiredSingle = null;
        principalPopulated.OptionalMany.Clear();
        principalPopulated.RequiredMany.Clear();

        Assert.Equal(12, context.ChangeTracker.Entries().Count());

        context.SaveChanges();

        Assert.Equal(9, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.Clear();

        LoadAndValidateEmpty();
        LoadAndValidatePopulated(1);

        Assert.Equal(6, context.ChangeTracker.Entries().Count());

        Assert.Equal(2, context.Set<OptionalSingleShortByte>().ToList().Count);
        Assert.Equal(1, context.Set<RequiredSingleShortByte>().ToList().Count);
        Assert.Equal(3, context.Set<OptionalManyShortByte>().ToList().Count);
        Assert.Equal(1, context.Set<RequiredManyShortByte>().ToList().Count);

        Assert.Equal(9, context.ChangeTracker.Entries().Count());

        PrincipalShortByte LoadAndValidateEmpty()
        {
            var loaded = Load().Single(e => e.OptionalSingle == null);

            Assert.Null(loaded.OptionalSingle);
            Assert.Null(loaded.RequiredSingle);
            Assert.Empty(loaded.OptionalMany);
            Assert.Empty(loaded.RequiredMany);

            return loaded;
        }

        PrincipalShortByte LoadAndValidatePopulated(int expected)
        {
            var loaded = Load().Single(e => e.OptionalSingle != null);

            Assert.NotNull(loaded.OptionalSingle);
            Assert.NotNull(loaded.RequiredSingle);
            Assert.Equal(expected, loaded.OptionalMany.Count);
            Assert.Equal(expected, loaded.RequiredMany.Count);

            return loaded;
        }

        IQueryable<PrincipalShortByte> Load()
            => context.ShortBytes
                .Include(e => e.OptionalMany)
                .Include(e => e.OptionalSingle)
                .Include(e => e.RequiredMany)
                .Include(e => e.RequiredSingle);
    }

    [ConditionalFact] // Issue #28392
    public virtual void Can_update_and_delete_with_string_FK_and_GUID_PK()
    {
        using var context = new MismatchedKeyTypesContext(Fixture);
        context.Database.BeginTransaction();

        var principalEmpty = LoadAndValidateEmpty();
        var principalPopulated = LoadAndValidatePopulated(2);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());

        principalEmpty.OptionalSingle = new OptionalSingleStringGuid();
        principalEmpty.RequiredSingle = new RequiredSingleStringGuid();
        principalEmpty.OptionalMany.Add(new OptionalManyStringGuid());
        principalEmpty.RequiredMany.Add(new RequiredManyStringGuid());

        principalPopulated.OptionalSingle = null;
        principalPopulated.RequiredSingle = null;
        principalPopulated.OptionalMany.Clear();
        principalPopulated.RequiredMany.Clear();

        Assert.Equal(12, context.ChangeTracker.Entries().Count());

        context.SaveChanges();

        Assert.Equal(9, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.Clear();

        LoadAndValidateEmpty();
        LoadAndValidatePopulated(1);

        Assert.Equal(6, context.ChangeTracker.Entries().Count());

        Assert.Equal(2, context.Set<OptionalSingleStringGuid>().ToList().Count);
        Assert.Equal(1, context.Set<RequiredSingleStringGuid>().ToList().Count);
        Assert.Equal(3, context.Set<OptionalManyStringGuid>().ToList().Count);
        Assert.Equal(1, context.Set<RequiredManyStringGuid>().ToList().Count);

        Assert.Equal(9, context.ChangeTracker.Entries().Count());

        PrincipalStringGuid LoadAndValidateEmpty()
        {
            var loaded = Load().Single(e => e.OptionalSingle == null);

            Assert.Null(loaded.OptionalSingle);
            Assert.Null(loaded.RequiredSingle);
            Assert.Empty(loaded.OptionalMany);
            Assert.Empty(loaded.RequiredMany);

            return loaded;
        }

        PrincipalStringGuid LoadAndValidatePopulated(int expected)
        {
            var loaded = Load().Single(e => e.OptionalSingle != null);

            Assert.NotNull(loaded.OptionalSingle);
            Assert.NotNull(loaded.RequiredSingle);
            Assert.Equal(expected, loaded.OptionalMany.Count);
            Assert.Equal(expected, loaded.RequiredMany.Count);

            return loaded;
        }

        IQueryable<PrincipalStringGuid> Load()
            => context.StringGuids
                .Include(e => e.OptionalMany)
                .Include(e => e.OptionalSingle)
                .Include(e => e.RequiredMany)
                .Include(e => e.RequiredSingle);
    }

    [ConditionalFact] // Issue #28392
    public virtual void Can_update_and_delete_composite_keys_mismatched_in_store()
    {
        using var context = new MismatchedKeyTypesContext(Fixture);
        context.Database.BeginTransaction();

        var principalEmpty = LoadAndValidateEmpty();
        var principalPopulated = LoadAndValidatePopulated(2);

        Assert.Equal(8, context.ChangeTracker.Entries().Count());

        principalEmpty.OptionalSingle = new OptionalSingleComposite();
        principalEmpty.RequiredSingle = new RequiredSingleComposite();
        principalEmpty.OptionalMany.Add(new OptionalManyComposite());
        principalEmpty.RequiredMany.Add(new RequiredManyComposite());

        principalPopulated.OptionalSingle = null;
        principalPopulated.RequiredSingle = null;
        principalPopulated.OptionalMany.Clear();
        principalPopulated.RequiredMany.Clear();

        Assert.Equal(12, context.ChangeTracker.Entries().Count());

        context.SaveChanges();

        Assert.Equal(9, context.ChangeTracker.Entries().Count());

        context.ChangeTracker.Clear();

        LoadAndValidateEmpty();
        LoadAndValidatePopulated(1);

        Assert.Equal(6, context.ChangeTracker.Entries().Count());

        Assert.Equal(2, context.Set<OptionalSingleComposite>().ToList().Count);
        Assert.Equal(1, context.Set<RequiredSingleComposite>().ToList().Count);
        Assert.Equal(3, context.Set<OptionalManyComposite>().ToList().Count);
        Assert.Equal(1, context.Set<RequiredManyComposite>().ToList().Count);

        Assert.Equal(9, context.ChangeTracker.Entries().Count());

        PrincipalComposite LoadAndValidateEmpty()
        {
            var loaded = Load().Single(e => e.OptionalSingle == null);

            Assert.Null(loaded.OptionalSingle);
            Assert.Null(loaded.RequiredSingle);
            Assert.Empty(loaded.OptionalMany);
            Assert.Empty(loaded.RequiredMany);

            return loaded;
        }

        PrincipalComposite LoadAndValidatePopulated(int expected)
        {
            var loaded = Load().Single(e => e.OptionalSingle != null);

            Assert.NotNull(loaded.OptionalSingle);
            Assert.NotNull(loaded.RequiredSingle);
            Assert.Equal(expected, loaded.OptionalMany.Count);
            Assert.Equal(expected, loaded.RequiredMany.Count);

            return loaded;
        }

        IQueryable<PrincipalComposite> Load()
            => context.Composites
                .Include(e => e.OptionalMany)
                .Include(e => e.OptionalSingle)
                .Include(e => e.RequiredMany)
                .Include(e => e.RequiredSingle);
    }

    [ConditionalFact]
    public virtual void Queries_work_but_SaveChanges_fails_when_composite_keys_incompatible_in_store()
    {
        using var context = new MismatchedKeyTypesContext(Fixture);
        context.Database.BeginTransaction();

        context.Database.ExecuteSqlRaw(
            @"INSERT INTO PrincipalBadComposite (Id1, Id2, Id3)
              VALUES (1, '833e6739-6ffb-4901-835c-b46d3f440c47', 1)");

        context.Database.ExecuteSqlRaw(
            @"INSERT INTO OptionalSingleBadComposite (Id, PrincipalId1, PrincipalId2, PrincipalId3)
              VALUES (1, 1, '833e6739-6ffb-4901-835c-b46d3f440c47', '4161c5b5-0b6c-4907-8534-2263737843a4')");

        var principal = context.Set<PrincipalBadComposite>().Single();
        var dependent = context.Set<OptionalSingleBadComposite>().Single();

        Assert.Same(dependent, principal.OptionalSingle);
        Assert.Same(principal, dependent.Principal);

        context.Entry(dependent).State = EntityState.Modified;

        Assert.Equal(
            RelationalStrings.StoredKeyTypesNotConvertable(
                nameof(OptionalSingleBadComposite.PrincipalId3), "uniqueidentifier", "int", nameof(PrincipalBadComposite.Id3)),
            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
    }

    [ConditionalFact]
    public virtual void Queries_work_but_SaveChanges_fails_when_keys_incompatible_in_store()
    {
        using var context = new MismatchedKeyTypesContext(Fixture);
        context.Database.BeginTransaction();

        context.Database.ExecuteSqlRaw(
            @"INSERT INTO PrincipalBad (Id)
              VALUES (1)");

        context.Database.ExecuteSqlRaw(
            @"INSERT INTO OptionalSingleBad (Id, PrincipalId)
              VALUES (1, '4161c5b5-0b6c-4907-8534-2263737843a4')");

        var principal = context.Set<PrincipalBad>().Single();
        var dependent = context.Set<OptionalSingleBad>().Single();

        Assert.Same(dependent, principal.OptionalSingle);
        Assert.Same(principal, dependent.Principal);

        context.Entry(dependent).State = EntityState.Modified;

        Assert.Equal(
            RelationalStrings.StoredKeyTypesNotConvertable(
                nameof(OptionalSingleBad.PrincipalId), "uniqueidentifier", "bigint", nameof(PrincipalBad.Id)),
            Assert.Throws<TargetInvocationException>(() => context.SaveChanges()).InnerException!.InnerException!.Message);
    }

    protected class MismatchedKeyTypesContextNoFks(MismatchedKeyTypesSqlServerFixture fixture) : MismatchedKeyTypesContext(fixture)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PrincipalIntLong>(
                b =>
                {
                    b.Ignore(e => e.OptionalSingle);
                    b.Ignore(e => e.RequiredSingle);
                    b.Ignore(e => e.OptionalMany);
                    b.Ignore(e => e.RequiredMany);
                });

            modelBuilder.Entity<OptionalSingleIntLong>().Ignore(e => e.Principal);
            modelBuilder.Entity<RequiredSingleIntLong>().Ignore(e => e.Principal);
            modelBuilder.Entity<OptionalManyIntLong>().Ignore(e => e.Principal);
            modelBuilder.Entity<RequiredManyIntLong>().Ignore(e => e.Principal);

            modelBuilder.Entity<PrincipalShortByte>(
                b =>
                {
                    b.Ignore(e => e.OptionalSingle);
                    b.Ignore(e => e.RequiredSingle);
                    b.Ignore(e => e.OptionalMany);
                    b.Ignore(e => e.RequiredMany);
                });

            modelBuilder.Entity<OptionalSingleShortByte>().Ignore(e => e.Principal);
            modelBuilder.Entity<RequiredSingleShortByte>().Ignore(e => e.Principal);
            modelBuilder.Entity<OptionalManyShortByte>().Ignore(e => e.Principal);
            modelBuilder.Entity<RequiredManyShortByte>().Ignore(e => e.Principal);

            modelBuilder.Entity<PrincipalStringGuid>(
                b =>
                {
                    b.Ignore(e => e.OptionalSingle);
                    b.Ignore(e => e.RequiredSingle);
                    b.Ignore(e => e.OptionalMany);
                    b.Ignore(e => e.RequiredMany);
                });

            modelBuilder.Entity<OptionalSingleStringGuid>().Ignore(e => e.Principal);
            modelBuilder.Entity<RequiredSingleStringGuid>().Ignore(e => e.Principal);
            modelBuilder.Entity<OptionalManyStringGuid>().Ignore(e => e.Principal);
            modelBuilder.Entity<RequiredManyStringGuid>().Ignore(e => e.Principal);

            modelBuilder.Entity<PrincipalComposite>(
                b =>
                {
                    b.Ignore(e => e.OptionalSingle);
                    b.Ignore(e => e.RequiredSingle);
                    b.Ignore(e => e.OptionalMany);
                    b.Ignore(e => e.RequiredMany);
                });

            modelBuilder.Entity<OptionalSingleComposite>().Ignore(e => e.Principal);
            modelBuilder.Entity<RequiredSingleComposite>().Ignore(e => e.Principal);
            modelBuilder.Entity<OptionalManyComposite>().Ignore(e => e.Principal);
            modelBuilder.Entity<RequiredManyComposite>().Ignore(e => e.Principal);

            modelBuilder.Entity<PrincipalBadComposite>(
                b =>
                {
                    b.Ignore(e => e.OptionalSingle);
                });

            modelBuilder.Entity<OptionalSingleBadComposite>().Ignore(e => e.Principal);

            modelBuilder.Entity<PrincipalBad>(
                b =>
                {
                    b.Ignore(e => e.OptionalSingle);
                });

            modelBuilder.Entity<OptionalSingleBad>().Ignore(e => e.Principal);

            base.OnModelCreating(modelBuilder);
        }
    }

    protected class MismatchedKeyTypesContext(MismatchedKeyTypesSqlServerFixture fixture) : DbContext
    {
        public MismatchedKeyTypesSqlServerFixture Fixture { get; } = fixture;

        public DbSet<PrincipalIntLong> IntLongs
            => Set<PrincipalIntLong>();

        public DbSet<PrincipalShortByte> ShortBytes
            => Set<PrincipalShortByte>();

        public DbSet<PrincipalStringGuid> StringGuids
            => Set<PrincipalStringGuid>();

        public DbSet<PrincipalComposite> Composites
            => Set<PrincipalComposite>();

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(Fixture.Store.ConnectionString);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PrincipalShortByte>()
                .Property(e => e.Id)
                .HasColumnType("smallint")
                .HasValueGenerator<TemporaryByteValueGenerator>();

            modelBuilder.Entity<PrincipalComposite>()
                .HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2,
                        e.Id3
                    });

            modelBuilder.Entity<PrincipalBadComposite>()
                .HasKey(
                    e => new
                    {
                        e.Id1,
                        e.Id2,
                        e.Id3
                    });

            modelBuilder.Entity<PrincipalBad>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<OptionalSingleIntLong>().Property(e => e.PrincipalId).HasColumnType("bigint");
            modelBuilder.Entity<RequiredSingleIntLong>().Property(e => e.PrincipalId).HasColumnType("bigint");
            modelBuilder.Entity<OptionalManyIntLong>().Property(e => e.PrincipalId).HasColumnType("bigint");
            modelBuilder.Entity<RequiredManyIntLong>().Property(e => e.PrincipalId).HasColumnType("bigint");
            modelBuilder.Entity<OptionalSingleShortByte>().Property(e => e.PrincipalId).HasColumnType("tinyint");
            modelBuilder.Entity<RequiredSingleShortByte>().Property(e => e.PrincipalId).HasColumnType("tinyint");
            modelBuilder.Entity<OptionalManyShortByte>().Property(e => e.PrincipalId).HasColumnType("tinyint");
            modelBuilder.Entity<RequiredManyShortByte>().Property(e => e.PrincipalId).HasColumnType("tinyint");
            modelBuilder.Entity<OptionalSingleStringGuid>().Property(e => e.PrincipalId).HasColumnType("nvarchar(64)");
            modelBuilder.Entity<RequiredSingleStringGuid>().Property(e => e.PrincipalId).HasColumnType("nvarchar(64)");
            modelBuilder.Entity<OptionalManyStringGuid>().Property(e => e.PrincipalId).HasColumnType("nvarchar(64)");
            modelBuilder.Entity<RequiredManyStringGuid>().Property(e => e.PrincipalId).HasColumnType("nvarchar(64)");
            modelBuilder.Entity<OptionalSingleComposite>().Property(e => e.PrincipalId1).HasColumnType("int");
            modelBuilder.Entity<RequiredSingleComposite>().Property(e => e.PrincipalId1).HasColumnType("int");
            modelBuilder.Entity<OptionalManyComposite>().Property(e => e.PrincipalId1).HasColumnType("int");
            modelBuilder.Entity<RequiredManyComposite>().Property(e => e.PrincipalId1).HasColumnType("int");
            modelBuilder.Entity<OptionalSingleComposite>().Property(e => e.PrincipalId2).HasColumnType("nvarchar(64)");
            modelBuilder.Entity<RequiredSingleComposite>().Property(e => e.PrincipalId2).HasColumnType("nvarchar(64)");
            modelBuilder.Entity<OptionalManyComposite>().Property(e => e.PrincipalId2).HasColumnType("nvarchar(64)");
            modelBuilder.Entity<RequiredManyComposite>().Property(e => e.PrincipalId2).HasColumnType("nvarchar(64)");

            modelBuilder.Entity<OptionalSingleBad>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.PrincipalId).HasConversion(v => new Guid(), v => 1);
                });

            modelBuilder.Entity<OptionalSingleBadComposite>(
                b =>
                {
                    b.Property(e => e.Id).ValueGeneratedNever();
                    b.Property(e => e.PrincipalId3).HasConversion(v => new Guid(), v => 1);
                });
        }
    }

    protected class PrincipalIntLong
    {
        public int Id { get; set; }

        public OptionalSingleIntLong? OptionalSingle { get; set; }
        public RequiredSingleIntLong? RequiredSingle { get; set; }
        public List<OptionalManyIntLong> OptionalMany { get; } = [];
        public List<RequiredManyIntLong> RequiredMany { get; } = [];
    }

    protected class OptionalSingleIntLong
    {
        public long Id { get; set; }

        public PrincipalIntLong? Principal { get; set; }
        public int? PrincipalId { get; set; }
    }

    protected class RequiredSingleIntLong
    {
        public Guid Id { get; set; }

        public PrincipalIntLong Principal { get; set; } = null!;
        public int PrincipalId { get; set; }
    }

    protected class OptionalManyIntLong
    {
        public long Id { get; set; }

        public PrincipalIntLong? Principal { get; set; }
        public int? PrincipalId { get; set; }
    }

    protected class RequiredManyIntLong
    {
        public Guid Id { get; set; }

        public PrincipalIntLong Principal { get; set; } = null!;
        public int PrincipalId { get; set; }
    }

    protected class PrincipalShortByte
    {
        public int Id { get; set; }

        public OptionalSingleShortByte? OptionalSingle { get; set; }
        public RequiredSingleShortByte? RequiredSingle { get; set; }
        public List<OptionalManyShortByte> OptionalMany { get; } = [];
        public List<RequiredManyShortByte> RequiredMany { get; } = [];
    }

    protected class OptionalSingleShortByte
    {
        public long Id { get; set; }

        public PrincipalShortByte? Principal { get; set; }
        public int? PrincipalId { get; set; }
    }

    protected class RequiredSingleShortByte
    {
        public Guid Id { get; set; }

        public PrincipalShortByte Principal { get; set; } = null!;
        public int PrincipalId { get; set; }
    }

    protected class OptionalManyShortByte
    {
        public long Id { get; set; }

        public PrincipalShortByte? Principal { get; set; }
        public int? PrincipalId { get; set; }
    }

    protected class RequiredManyShortByte
    {
        public Guid Id { get; set; }

        public PrincipalShortByte Principal { get; set; } = null!;
        public int PrincipalId { get; set; }
    }

    protected class PrincipalStringGuid
    {
        public Guid Id { get; set; }

        public OptionalSingleStringGuid? OptionalSingle { get; set; }
        public RequiredSingleStringGuid? RequiredSingle { get; set; }
        public List<OptionalManyStringGuid> OptionalMany { get; } = [];
        public List<RequiredManyStringGuid> RequiredMany { get; } = [];
    }

    protected class OptionalSingleStringGuid
    {
        public short Id { get; set; }

        public PrincipalStringGuid? Principal { get; set; }
        public Guid? PrincipalId { get; set; }
    }

    protected class RequiredSingleStringGuid
    {
        public short Id { get; set; }

        public PrincipalStringGuid Principal { get; set; } = null!;
        public Guid PrincipalId { get; set; }
    }

    protected class OptionalManyStringGuid
    {
        public short Id { get; set; }

        public PrincipalStringGuid? Principal { get; set; }
        public Guid? PrincipalId { get; set; }
    }

    protected class RequiredManyStringGuid
    {
        public short Id { get; set; }

        public PrincipalStringGuid Principal { get; set; } = null!;
        public Guid PrincipalId { get; set; }
    }

    protected class PrincipalComposite
    {
        public long Id1 { get; set; }
        public Guid Id2 { get; set; }
        public int Id3 { get; set; }

        public OptionalSingleComposite? OptionalSingle { get; set; }
        public RequiredSingleComposite? RequiredSingle { get; set; }
        public List<OptionalManyComposite> OptionalMany { get; } = [];
        public List<RequiredManyComposite> RequiredMany { get; } = [];
    }

    protected class OptionalSingleComposite
    {
        public int Id { get; set; }

        public PrincipalComposite? Principal { get; set; }
        public long? PrincipalId1 { get; set; }
        public Guid? PrincipalId2 { get; set; }
        public int? PrincipalId3 { get; set; }
    }

    protected class RequiredSingleComposite
    {
        public int Id { get; set; }

        public PrincipalComposite Principal { get; set; } = null!;
        public long PrincipalId1 { get; set; }
        public Guid PrincipalId2 { get; set; }
        public int PrincipalId3 { get; set; }
    }

    protected class OptionalManyComposite
    {
        public int Id { get; set; }

        public PrincipalComposite? Principal { get; set; }
        public long? PrincipalId1 { get; set; }
        public Guid? PrincipalId2 { get; set; }
        public int? PrincipalId3 { get; set; }
    }

    protected class RequiredManyComposite
    {
        public int Id { get; set; }

        public PrincipalComposite Principal { get; set; } = null!;
        public long PrincipalId1 { get; set; }
        public Guid PrincipalId2 { get; set; }
        public int PrincipalId3 { get; set; }
    }

    protected class PrincipalBad
    {
        public long Id { get; set; }

        public OptionalSingleBad? OptionalSingle { get; set; }
    }

    protected class OptionalSingleBad
    {
        public int Id { get; set; }

        public PrincipalBad? Principal { get; set; }
        public long? PrincipalId { get; set; }
    }

    protected class PrincipalBadComposite
    {
        public long Id1 { get; set; }
        public Guid Id2 { get; set; }
        public int Id3 { get; set; }

        public OptionalSingleBadComposite? OptionalSingle { get; set; }
    }

    protected class OptionalSingleBadComposite
    {
        public int Id { get; set; }

        public PrincipalBadComposite? Principal { get; set; }
        public long? PrincipalId1 { get; set; }
        public Guid? PrincipalId2 { get; set; }
        public int? PrincipalId3 { get; set; }
    }

    public class MismatchedKeyTypesSqlServerFixture : IAsyncLifetime
    {
        public async Task SeedAsync()
        {
            using var context = new MismatchedKeyTypesContext(this);

            context.AddRange(
                new PrincipalIntLong
                {
                    OptionalSingle = new OptionalSingleIntLong(),
                    RequiredSingle = new RequiredSingleIntLong(),
                    OptionalMany = { new OptionalManyIntLong(), new OptionalManyIntLong() },
                    RequiredMany = { new RequiredManyIntLong(), new RequiredManyIntLong() }
                },
                new PrincipalIntLong(),
                new PrincipalShortByte
                {
                    OptionalSingle = new OptionalSingleShortByte(),
                    RequiredSingle = new RequiredSingleShortByte(),
                    OptionalMany = { new OptionalManyShortByte(), new OptionalManyShortByte() },
                    RequiredMany = { new RequiredManyShortByte(), new RequiredManyShortByte() }
                },
                new PrincipalShortByte(),
                new PrincipalStringGuid
                {
                    OptionalSingle = new OptionalSingleStringGuid(),
                    RequiredSingle = new RequiredSingleStringGuid(),
                    OptionalMany = { new OptionalManyStringGuid(), new OptionalManyStringGuid() },
                    RequiredMany = { new RequiredManyStringGuid(), new RequiredManyStringGuid() }
                },
                new PrincipalStringGuid(),
                new PrincipalComposite
                {
                    Id1 = 1,
                    Id2 = Guid.NewGuid(),
                    Id3 = -1,
                    OptionalSingle = new OptionalSingleComposite(),
                    RequiredSingle = new RequiredSingleComposite(),
                    OptionalMany = { new OptionalManyComposite(), new OptionalManyComposite() },
                    RequiredMany = { new RequiredManyComposite(), new RequiredManyComposite() }
                },
                new PrincipalComposite
                {
                    Id1 = 1,
                    Id2 = Guid.NewGuid(),
                    Id3 = -1
                });

            await context.SaveChangesAsync();
        }

        public SqlServerTestStore Store { get; set; } = null!;

        public async Task InitializeAsync()
        {
            Store = await SqlServerTestStore.CreateInitializedAsync("MismatchedKeyTypes");

            using (var context = new MismatchedKeyTypesContextNoFks(this))
            {
                context.Database.EnsureClean();
            }

            await SeedAsync();
        }

        public Task DisposeAsync()
        {
            Store.Dispose();
            return Task.CompletedTask;
        }
    }

    private class TemporaryByteValueGenerator : ValueGenerator<int>
    {
        private int _current;

        public override int Next(EntityEntry entry)
            => (byte)Interlocked.Decrement(ref _current);

        public override bool GeneratesTemporaryValues
            => true;
    }
}
