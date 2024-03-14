// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.SqlServer.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class SqlServerValueGenerationScenariosTestBase
{
    protected static readonly GeometryFactory GeometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);

    protected abstract string DatabaseName { get; }

    protected abstract Guid GuidSentinel { get; }
    protected abstract int IntSentinel { get; }
    protected abstract uint UIntSentinel { get; }
    protected abstract IntKey IntKeySentinel { get; }
    protected abstract ULongKey ULongKeySentinel { get; }
    protected abstract int? NullableIntSentinel { get; }
    protected abstract string StringSentinel { get; }
    protected abstract DateTime DateTimeSentinel { get; }
    protected abstract NeedsConverter NeedsConverterSentinel { get; }
    protected abstract GeometryCollection GeometryCollectionSentinel { get; }
    protected abstract byte[] TimestampSentinel { get; }

    // Positive cases

    [ConditionalFact]
    public async Task Insert_with_Identity_column()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextIdentity(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(CreateBlog("One Unicorn"), CreateBlog("Two Unicorns"));

            context.SaveChanges();
        }

        using (var context = new BlogContextIdentity(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, blogs[0].Id);
            Assert.Equal(2, blogs[1].Id);
        }
    }

    public class BlogContextIdentity(string databaseName, Action<ModelBuilder> modelBuilder) : ContextBase(databaseName, modelBuilder);

    [ConditionalFact]
    public async Task Insert_with_sequence_HiLo()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextHiLo(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(CreateBlog("One Unicorn"), CreateBlog("Two Unicorns"));

            context.SaveChanges();
        }

        using (var context = new BlogContextHiLo(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, blogs[0].Id);
            Assert.Equal(2, blogs[0].OtherId);
            Assert.Equal(3, blogs[1].Id);
            Assert.Equal(4, blogs[1].OtherId);
        }
    }

    public class BlogContextHiLo(string databaseName, Action<ModelBuilder> modelBuilder) : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseHiLo();

            modelBuilder.Entity<Blog>(
                eb =>
                {
                    eb.HasAlternateKey(
                        b => new { b.OtherId });
                    eb.Property(b => b.OtherId).ValueGeneratedOnAdd();
                });
        }
    }

    [ConditionalFact]
    public async Task Insert_with_key_sequence()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextKeySequence(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(CreateBlog("One Unicorn"), CreateBlog("Two Unicorns"));

            context.SaveChanges();
        }

        using (var context = new BlogContextKeySequence(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, blogs[0].Id);
            Assert.Equal(1, blogs[0].OtherId);
            Assert.Equal(2, blogs[1].Id);
            Assert.Equal(2, blogs[1].OtherId);
        }
    }

    public class BlogContextKeySequence(string databaseName, Action<ModelBuilder> modelBuilder) : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.UseKeySequences();

            modelBuilder.Entity<Blog>(
                eb =>
                {
                    eb.HasAlternateKey(
                        b => new { b.OtherId });
                    eb.Property(b => b.OtherId).ValueGeneratedOnAdd();
                });
        }
    }

    [ConditionalFact]
    public async Task Insert_with_non_key_sequence()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextNonKeySequence(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(CreateBlog("One Unicorn"), CreateBlog("Two Unicorns"));

            context.SaveChanges();
        }

        using (var context = new BlogContextNonKeySequence(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, blogs[0].Id);
            Assert.Equal(1, blogs[0].OtherId);
            Assert.Equal(2, blogs[1].Id);
            Assert.Equal(2, blogs[1].OtherId);
        }
    }

    public class BlogContextNonKeySequence(string databaseName, Action<ModelBuilder> modelBuilder) : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>(
                eb =>
                {
                    eb.Property(b => b.OtherId).UseSequence();
                    eb.Property(b => b.OtherId).ValueGeneratedOnAdd();
                });
        }
    }

    [ConditionalFact]
    public async Task Insert_with_default_value_from_sequence()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextDefaultValue(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(CreateBlog("One Unicorn"), CreateBlog("Two Unicorns"));

            context.SaveChanges();
        }

        using (var context = new BlogContextDefaultValue(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(1, blogs[1].Id);
        }

        using (var context = new BlogContextDefaultValueNoMigrations(testStore.Name, OnModelCreating))
        {
            context.AddRange(CreateBlog("One Unicorn"), CreateBlog("Two Unicorns"));

            context.SaveChanges();
        }

        using (var context = new BlogContextDefaultValueNoMigrations(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(1, blogs[1].Id);
            Assert.Equal(2, blogs[2].Id);
            Assert.Equal(3, blogs[3].Id);
        }
    }

    public class BlogContextDefaultValue(string databaseName, Action<ModelBuilder> modelBuilder) : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasSequence("MySequence")
                .StartsAt(0);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .HasDefaultValueSql("next value for MySequence");
        }
    }

    public class BlogContextDefaultValueNoMigrations(string databaseName, Action<ModelBuilder> modelBuilder)
        : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .HasDefaultValue();
        }
    }

    [ConditionalFact]
    public async Task Insert_with_default_string_value_from_sequence()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextStringDefaultValue(testStore.Name, OnModelCreating, StringSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new BlogWithStringKey { Id = StringSentinel, Name = "One Unicorn" },
                new BlogWithStringKey { Id = StringSentinel, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextStringDefaultValue(testStore.Name, OnModelCreating, StringSentinel))
        {
            var blogs = context.StringyBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal("i77", blogs[0].Id);
            Assert.Equal("i78", blogs[1].Id);
        }
    }

    public class BlogContextStringDefaultValue(string databaseName, Action<ModelBuilder> modelBuilder, string stringSentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly string _stringSentinel = stringSentinel;

        public DbSet<BlogWithStringKey> StringyBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasSequence("MyStringSequence")
                .StartsAt(77);

            modelBuilder
                .Entity<BlogWithStringKey>()
                .Property(e => e.Id)
                .HasDefaultValueSql("'i' + CAST((NEXT VALUE FOR MyStringSequence) AS VARCHAR(20))")
                .HasSentinel(_stringSentinel);
        }
    }

    public class BlogWithStringKey
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }

    [ConditionalFact]
    public async Task Insert_with_key_default_value_from_sequence()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextKeyColumnWithDefaultValue(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(CreateBlog("One Unicorn"), CreateBlog("Two Unicorns"));

            context.SaveChanges();
        }

        using (var context = new BlogContextKeyColumnWithDefaultValue(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(77, blogs[0].Id);
            Assert.Equal(78, blogs[1].Id);
        }
    }

    public class BlogContextKeyColumnWithDefaultValue(string databaseName, Action<ModelBuilder> modelBuilder)
        : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .HasSequence("MySequence")
                .StartsAt(77);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .HasDefaultValueSql("next value for MySequence")
                .Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
        }
    }

    [ConditionalFact]
    public async Task Insert_uint_to_Identity_column_using_value_converter()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextUIntToIdentityUsingValueConverter(testStore.Name, OnModelCreating, UIntSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new BlogWithUIntKey { Id = UIntSentinel, Name = "One Unicorn" },
                new BlogWithUIntKey { Id = UIntSentinel, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextUIntToIdentityUsingValueConverter(testStore.Name, OnModelCreating, UIntSentinel))
        {
            var blogs = context.UnsignedBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal((uint)1, blogs[0].Id);
            Assert.Equal((uint)2, blogs[1].Id);
        }
    }

    public class BlogContextUIntToIdentityUsingValueConverter(string databaseName, Action<ModelBuilder> modelBuilder, uint uintSentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly uint _uintSentinel = uintSentinel;

        public DbSet<BlogWithUIntKey> UnsignedBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BlogWithUIntKey>()
                .Property(e => e.Id)
                .HasConversion<int>()
                .HasSentinel(_uintSentinel);
        }
    }

    public class BlogWithUIntKey
    {
        public uint Id { get; set; }
        public string Name { get; set; }
    }

    [ConditionalFact]
    public async Task Insert_int_enum_to_Identity_column()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextIntEnumToIdentity(testStore.Name, OnModelCreating, IntKeySentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new BlogWithIntEnumKey { Id = IntKeySentinel, Name = "One Unicorn" },
                new BlogWithIntEnumKey { Id = IntKeySentinel, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextIntEnumToIdentity(testStore.Name, OnModelCreating, IntKeySentinel))
        {
            var blogs = context.EnumBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, (int)blogs[0].Id);
            Assert.Equal(2, (int)blogs[1].Id);
        }
    }

    public class BlogContextIntEnumToIdentity(string databaseName, Action<ModelBuilder> modelBuilder, IntKey sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly IntKey _sentinel = sentinel;

        public DbSet<BlogWithIntEnumKey> EnumBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BlogWithIntEnumKey>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasSentinel(_sentinel);
        }
    }

    public class BlogWithIntEnumKey
    {
        public IntKey Id { get; set; }
        public string Name { get; set; }
    }

    public enum IntKey
    {
        Zero,
        One,
        SixSixSeven,
    }

    [ConditionalFact]
    public async Task Insert_ulong_enum_to_Identity_column()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextULongEnumToIdentity(testStore.Name, OnModelCreating, ULongKeySentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new BlogWithULongEnumKey { Id = ULongKeySentinel, Name = "One Unicorn" },
                new BlogWithULongEnumKey { Id = ULongKeySentinel, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextULongEnumToIdentity(testStore.Name, OnModelCreating, ULongKeySentinel))
        {
            var blogs = context.EnumBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(1, (int)blogs[0].Id);
            Assert.Equal(2, (int)blogs[1].Id);
        }
    }

    public class BlogContextULongEnumToIdentity(string databaseName, Action<ModelBuilder> modelBuilder, ULongKey sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly ULongKey _sentinel = sentinel;

        public DbSet<BlogWithULongEnumKey> EnumBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<BlogWithULongEnumKey>()
                .Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasSentinel(_sentinel);
        }
    }

    public class BlogWithULongEnumKey
    {
        public ULongKey Id { get; set; }
        public string Name { get; set; }
    }

    public enum ULongKey : ulong
    {
        Zero,
        Sentinel
    }

    [ConditionalFact]
    public async Task Insert_string_to_Identity_column_using_value_converter()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextStringToIdentityUsingValueConverter(testStore.Name, OnModelCreating, StringSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new BlogWithStringKey { Id = StringSentinel, Name = "One Unicorn" },
                new BlogWithStringKey { Id = StringSentinel, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextStringToIdentityUsingValueConverter(testStore.Name, OnModelCreating, StringSentinel))
        {
            var blogs = context.StringyBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal("1", blogs[0].Id);
            Assert.Equal("2", blogs[1].Id);
        }
    }

    public class BlogContextStringToIdentityUsingValueConverter(string databaseName, Action<ModelBuilder> modelBuilder, string sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly string _sentinel = sentinel;

        public DbSet<BlogWithStringKey> StringyBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            Guid guid;
            modelBuilder
                .Entity<BlogWithStringKey>()
                .Property(e => e.Id)
                .HasValueGenerator<TemporaryStringValueGenerator>()
                .HasConversion(
                    v => Guid.TryParse(v, out guid)
                        ? default
                        : int.Parse(v),
                    v => v.ToString())
                .ValueGeneratedOnAdd()
                .HasSentinel(_sentinel);
        }
    }

    [ConditionalFact]
    public async Task Insert_with_explicit_non_default_keys()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextNoKeyGeneration(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new Blog { Id = 66, Name = "One Unicorn" }, new Blog { Id = 67, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextNoKeyGeneration(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(66, blogs[0].Id);
            Assert.Equal(67, blogs[1].Id);
        }
    }

    public class BlogContextNoKeyGeneration(string databaseName, Action<ModelBuilder> modelBuilder)
        : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .ValueGeneratedNever();
        }
    }

    [ConditionalFact]
    public async Task Insert_with_explicit_with_default_keys()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextNoKeyGenerationNullableKey(testStore.Name, OnModelCreating, NullableIntSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new NullableKeyBlog { Id = 0, Name = "One Unicorn" },
                new NullableKeyBlog { Id = 1, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextNoKeyGenerationNullableKey(testStore.Name, OnModelCreating, NullableIntSentinel))
        {
            var blogs = context.NullableKeyBlogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(1, blogs[1].Id);
        }
    }

    public class BlogContextNoKeyGenerationNullableKey(string databaseName, Action<ModelBuilder> modelBuilder, int? sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly int? _sentinel = sentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<NullableKeyBlog>()
                .Property(e => e.Id)
                .ValueGeneratedNever()
                .HasSentinel(_sentinel);
        }
    }

    [ConditionalFact]
    public async Task Insert_with_non_key_default_value()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextNonKeyDefaultValue(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            var blogs = new List<Blog>
            {
                new()
                {
                    Id = IntSentinel,
                    Name = "One Unicorn",
                    CreatedOn = DateTimeSentinel,
                    NeedsConverter = NeedsConverterSentinel
                },
                new()
                {
                    Id = IntSentinel,
                    Name = "Two Unicorns",
                    CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0),
                    NeedsConverter = new NeedsConverter(111)
                }
            };

            context.AddRange(blogs);

            context.SaveChanges();

            Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
            Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);
            Assert.Equal(111, blogs[1].NeedsConverter.Value);
        }

        using (var context = new BlogContextNonKeyDefaultValue(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Name).ToList();
            Assert.Equal(3, blogs.Count);

            Assert.NotEqual(new DateTime(), blogs[0].CreatedOn);
            Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);
            Assert.Equal(new DateTime(1974, 8, 3, 0, 10, 0), blogs[2].CreatedOn);

            blogs[0].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);

            blogs[1].Name = "X Unicorns";
            blogs[1].NeedsConverter = new NeedsConverter(222);

            blogs[2].Name = "Y Unicorns";
            blogs[2].NeedsConverter = new NeedsConverter(333);

            context.SaveChanges();
        }

        using (var context = new BlogContextNonKeyDefaultValue(testStore.Name, OnModelCreating))
        {
            var blogs = context.Blogs.OrderBy(e => e.Name).ToList();
            Assert.Equal(3, blogs.Count);

            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[0].CreatedOn);
            Assert.Equal(new DateTime(1969, 8, 3, 0, 10, 0), blogs[1].CreatedOn);
            Assert.Equal(222, blogs[1].NeedsConverter.Value);
            Assert.Equal(new DateTime(1974, 8, 3, 0, 10, 0), blogs[2].CreatedOn);
            Assert.Equal(333, blogs[2].NeedsConverter.Value);
        }
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsSqlClr)]
    public async Task Insert_with_non_key_default_spatial_value()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);

        using (var context = new BlogContextNonKeyDefaultSpatialValue(testStore.Name, OnModelCreating))
        {
            context.Database.EnsureCreatedResiliently();

            var blogs = new List<BlogWithSpatial>
            {
                new()
                {
                    Id = IntSentinel,
                    Name = "One Unicorn",
                    GeometryCollection = GeometryCollectionSentinel
                },
                new()
                {
                    Id = IntSentinel,
                    Name = "Two Unicorns",
                    GeometryCollection = GeometryFactory.CreateGeometryCollection(
                        [GeometryFactory.CreatePoint(new Coordinate(1, 3))])
                }
            };

            context.AddRange(blogs);

            context.SaveChanges();

            var point = ((Point)blogs[1].GeometryCollection.Geometries[0]);
            Assert.Equal(1, point.X);
            Assert.Equal(3, point.Y);
        }

        using (var context = new BlogContextNonKeyDefaultSpatialValue(testStore.Name, OnModelCreating))
        {
            var blogs = context.SpatialBlogs.OrderBy(e => e.Name).ToList();
            Assert.Equal(3, blogs.Count);

            var point1 = ((Point)blogs[1].GeometryCollection.Geometries[0]);
            Assert.Equal(1, point1.X);
            Assert.Equal(3, point1.Y);

            var point2 = ((Point)blogs[2].GeometryCollection.Geometries[0]);
            Assert.Equal(1, point2.X);
            Assert.Equal(2, point2.Y);

            blogs[1].GeometryCollection.Geometries[0] = GeometryFactory.CreatePoint(new Coordinate(1, 11));
            blogs[2].GeometryCollection.Geometries[0] = GeometryFactory.CreatePoint(new Coordinate(1, 22));

            context.SaveChanges();
        }

        using (var context = new BlogContextNonKeyDefaultSpatialValue(testStore.Name, OnModelCreating))
        {
            var blogs = context.SpatialBlogs.OrderBy(e => e.Name).ToList();
            Assert.Equal(3, blogs.Count);

            var point1 = ((Point)blogs[1].GeometryCollection.Geometries[0]);
            Assert.Equal(1, point1.X);
            Assert.Equal(11, point1.Y);

            var point2 = ((Point)blogs[2].GeometryCollection.Geometries[0]);
            Assert.Equal(1, point2.X);
            Assert.Equal(22, point2.Y);
        }
    }

    public class BlogContextNonKeyDefaultValue(string databaseName, Action<ModelBuilder> modelBuilder)
        : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>(
                b =>
                {
                    b.Property(e => e.CreatedOn).HasDefaultValueSql("getdate()");

                    b.HasData(
                        new Blog
                        {
                            Id = 9979,
                            Name = "W Unicorns",
                            CreatedOn = new DateTime(1974, 8, 3, 0, 10, 0),
                            NeedsConverter = new NeedsConverter(111),
                        });
                });
        }
    }

    public class BlogContextNonKeyDefaultSpatialValue(string databaseName, Action<ModelBuilder> modelBuilder)
        : ContextBase(databaseName, modelBuilder)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BlogWithSpatial>(
                b =>
                {
                    b.Property(e => e.GeometryCollection).HasDefaultValue(GeometryFactory.CreateGeometryCollection());

                    b.HasData(
                        new BlogWithSpatial
                        {
                            Id = 9979,
                            Name = "W Unicorns",
                            GeometryCollection = GeometryFactory.CreateGeometryCollection(
                                [GeometryFactory.CreatePoint(new Coordinate(1, 2))])
                        });
                });
        }
    }

    [ConditionalFact]
    public async Task Insert_with_non_key_default_value_readonly()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name, OnModelCreating, IntSentinel, DateTimeSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new Blog
                {
                    Id = IntSentinel,
                    Name = "One Unicorn",
                    CreatedOn = DateTimeSentinel
                },
                new Blog
                {
                    Id = IntSentinel,
                    Name = "Two Unicorns",
                    CreatedOn = DateTimeSentinel
                });

            context.SaveChanges();

            Assert.NotEqual(new DateTime(), context.Blogs.ToList()[0].CreatedOn);
        }

        DateTime dateTime0;

        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name, OnModelCreating, IntSentinel, DateTimeSentinel))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            dateTime0 = blogs[0].CreatedOn;

            Assert.NotEqual(new DateTime(), dateTime0);
            Assert.NotEqual(new DateTime(), blogs[1].CreatedOn);

            blogs[0].Name = "One Pegasus";
            blogs[1].CreatedOn = new DateTime(1973, 9, 3, 0, 10, 0);

            context.SaveChanges();
        }

        using (var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name, OnModelCreating, IntSentinel, DateTimeSentinel))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(dateTime0, blogs[0].CreatedOn);
            Assert.Equal(new DateTime(1973, 9, 3, 0, 10, 0), blogs[1].CreatedOn);
        }
    }

    public class BlogContextNonKeyReadOnlyDefaultValue(
        string databaseName,
        Action<ModelBuilder> modelBuilder,
        int intSentinel,
        DateTime dateTimeSentinel) : ContextBase(databaseName, modelBuilder)
    {
        private readonly int _intSentinel = intSentinel;
        private readonly DateTime _dateTimeSentinel = dateTimeSentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Blog>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(_intSentinel);

                    var property = b.Property(e => e.CreatedOn).HasDefaultValueSql("getdate()").HasSentinel(_dateTimeSentinel);
                    property.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                });
        }
    }

    [ConditionalFact]
    public async Task Insert_and_update_with_computed_column()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            var blog = context.Add(
                new FullNameBlog
                {
                    Id = IntSentinel,
                    FirstName = "One",
                    LastName = "Unicorn",
                    FullName = StringSentinel
                }).Entity;

            context.SaveChanges();

            Assert.Equal("One Unicorn", blog.FullName);
        }

        using (var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            var blog = context.FullNameBlogs.Single();

            Assert.Equal("One Unicorn", blog.FullName);

            blog.LastName = "Pegasus";

            context.SaveChanges();

            Assert.Equal("One Pegasus", blog.FullName);
        }
    }

    public class BlogContextComputedColumn(string databaseName, Action<ModelBuilder> modelBuilder, int intSentinel, string stringSentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly int _intSentinel = intSentinel;
        private readonly string _stringSentinel = stringSentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FullNameBlog>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(_intSentinel);

                    var property = b.Property(e => e.FullName)
                        .HasComputedColumnSql("FirstName + ' ' + LastName")
                        .HasSentinel(_stringSentinel)
                        .Metadata;

                    property.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                });
        }
    }

    public class BlogContextComputedColumnWithTriggerMetadata(
        string databaseName,
        Action<ModelBuilder> modelBuilder,
        int intSentinel,
        string stringSentinel) : BlogContextComputedColumn(databaseName, modelBuilder, intSentinel, stringSentinel)
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FullNameBlog>().ToTable(tb => tb.HasTrigger("SomeTrigger"));
        }
    }

    // #6044
    [ConditionalFact]
    public async Task Insert_and_update_with_computed_column_with_function()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextComputedColumnWithFunction(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            context.Database.ExecuteSqlRaw
            (
                @"CREATE FUNCTION
[dbo].[GetFullName](@First NVARCHAR(MAX), @Second NVARCHAR(MAX))
RETURNS NVARCHAR(MAX) WITH SCHEMABINDING AS BEGIN RETURN @First + @Second END");

            context.GetService<IRelationalDatabaseCreator>().CreateTables();
        }

        using (var context = new BlogContextComputedColumnWithFunction(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            var blog = context.Add(
                new FullNameBlog
                {
                    Id = IntSentinel,
                    FirstName = "One",
                    LastName = "Unicorn",
                    FullName = StringSentinel
                }).Entity;

            context.SaveChanges();

            Assert.Equal("OneUnicorn", blog.FullName);
        }

        using (var context = new BlogContextComputedColumnWithFunction(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            var blog = context.FullNameBlogs.Single();

            Assert.Equal("OneUnicorn", blog.FullName);

            blog.LastName = "Pegasus";

            context.SaveChanges();

            Assert.Equal("OnePegasus", blog.FullName);
        }
    }

    public class BlogContextComputedColumnWithFunction(
        string databaseName,
        Action<ModelBuilder> modelBuilder,
        int intSentinel,
        string stringSentinel) : ContextBase(databaseName, modelBuilder)
    {
        private readonly int _intSentinel = intSentinel;
        private readonly string _stringSentinel = stringSentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<FullNameBlog>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(_intSentinel);

                    var property = modelBuilder.Entity<FullNameBlog>()
                        .Property(e => e.FullName)
                        .HasComputedColumnSql("[dbo].[GetFullName]([FirstName], [LastName])")
                        .HasSentinel(_stringSentinel)
                        .Metadata;

                    property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
                });
        }
    }

    // #6044
    [ConditionalFact]
    public async Task Insert_and_update_with_computed_column_with_querying_function()
    {
        SqlServerTestStore testStore = null;
        try
        {
            testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
            using (var context = new BlogContextComputedColumnWithTriggerMetadata(
                       testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
            {
                context.GetService<IRelationalDatabaseCreator>().CreateTables();

                context.Database.ExecuteSqlRaw("ALTER TABLE dbo.FullNameBlogs DROP COLUMN FullName;");

                context.Database.ExecuteSqlRaw(
                    @"CREATE FUNCTION [dbo].[GetFullName](@Id int)
RETURNS nvarchar(max) WITH SCHEMABINDING AS
BEGIN
    DECLARE @FullName nvarchar(max);
    SELECT @FullName = [FirstName] + [LastName] FROM [dbo].[FullNameBlogs] WHERE [Id] = @Id;
    RETURN @FullName
END");

                context.Database.ExecuteSqlRaw("ALTER TABLE dbo.FullNameBlogs ADD FullName AS [dbo].[GetFullName]([Id]); ");
            }

            using (var context = new BlogContextComputedColumnWithTriggerMetadata(
                       testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
            {
                var blog = context.Add(
                    new FullNameBlog
                    {
                        Id = IntSentinel,
                        FirstName = "One",
                        LastName = "Unicorn",
                        FullName = StringSentinel
                    }).Entity;

                context.SaveChanges();

                Assert.Equal("OneUnicorn", blog.FullName);
            }

            using (var context = new BlogContextComputedColumnWithTriggerMetadata(
                       testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
            {
                var blog = context.FullNameBlogs.Single();

                Assert.Equal("OneUnicorn", blog.FullName);

                blog.LastName = "Pegasus";

                context.SaveChanges();

                Assert.Equal("OnePegasus", blog.FullName);
            }

            using (var context = new BlogContextComputedColumnWithTriggerMetadata(
                       testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
            {
                var blog1 = context.Add(
                    new FullNameBlog
                    {
                        Id = IntSentinel,
                        FirstName = "Hank",
                        LastName = "Unicorn",
                        FullName = StringSentinel
                    }).Entity;

                var blog2 = context.Add(
                    new FullNameBlog
                    {
                        Id = IntSentinel,
                        FirstName = "Jeff",
                        LastName = "Unicorn",
                        FullName = StringSentinel
                    }).Entity;

                context.SaveChanges();

                Assert.Equal("HankUnicorn", blog1.FullName);
                Assert.Equal("JeffUnicorn", blog2.FullName);
            }
        }
        finally
        {
            using var context = new BlogContextComputedColumnWithTriggerMetadata(
                DatabaseName, OnModelCreating, IntSentinel, StringSentinel);
            context.Database.ExecuteSqlRaw("ALTER TABLE dbo.FullNameBlogs DROP COLUMN FullName;");
            context.Database.ExecuteSqlRaw("DROP FUNCTION [dbo].[GetFullName];");
            testStore?.Dispose();
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Insert_with_computed_column_with_function_without_metadata_configuration(bool async)
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();

            context.Database.ExecuteSqlRaw("ALTER TABLE dbo.FullNameBlogs DROP COLUMN FullName;");

            context.Database.ExecuteSqlRaw(
                @"CREATE FUNCTION [dbo].[GetFullName](@Id int)
RETURNS nvarchar(max) WITH SCHEMABINDING AS
BEGIN
    DECLARE @FullName nvarchar(max);
    SELECT @FullName = [FirstName] + [LastName] FROM [dbo].[FullNameBlogs] WHERE [Id] = @Id;
    RETURN @FullName
END");

            context.Database.ExecuteSqlRaw("ALTER TABLE dbo.FullNameBlogs ADD FullName AS [dbo].[GetFullName]([Id]); ");
        }

        try
        {
            using (var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
            {
                await context.AddAsync(new FullNameBlog { Id = IntSentinel, FullName = StringSentinel });

                var exception = async
                    ? await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())
                    : Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                Assert.Equal(SqlServerStrings.SaveChangesFailedBecauseOfComputedColumnWithFunction, exception.Message);

                var sqlException = Assert.IsType<SqlException>(exception.InnerException);
                Assert.Equal(4186, sqlException.Number);
            }
        }
        finally
        {
            using var context = new BlogContextComputedColumnWithTriggerMetadata(
                testStore.Name, OnModelCreating, IntSentinel, StringSentinel);
            context.Database.ExecuteSqlRaw("ALTER TABLE dbo.FullNameBlogs DROP COLUMN FullName;");
            context.Database.ExecuteSqlRaw("DROP FUNCTION [dbo].[GetFullName];");
        }
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public async Task Insert_with_trigger_without_metadata_configuration(bool async)
    {
        // Execute an insert against a table which has a trigger, but which haven't identified as such in our metadata.
        // This causes a specialized exception to be thrown, directing users to the relevant docs.
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            context.GetService<IRelationalDatabaseCreator>().CreateTables();

            context.Database.ExecuteSqlRaw(
                @"CREATE OR ALTER TRIGGER [FullNameBlogs_Trigger]
ON [FullNameBlogs]
FOR INSERT, UPDATE, DELETE AS
BEGIN
	IF @@ROWCOUNT = 0
		return
END");
        }

        try
        {
            using (var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
            {
                await context.AddAsync(new FullNameBlog { Id = IntSentinel, FullName = StringSentinel });

                var exception = async
                    ? await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync())
                    : Assert.Throws<DbUpdateException>(() => context.SaveChanges());

                Assert.Equal(SqlServerStrings.SaveChangesFailedBecauseOfTriggers, exception.Message);

                var sqlException = Assert.IsType<SqlException>(exception.InnerException);
                Assert.Equal(334, sqlException.Number);
            }
        }
        finally
        {
            using var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel);
            context.Database.ExecuteSqlRaw("DROP TRIGGER [FullNameBlogs_Trigger]");
        }
    }

    [ConditionalFact]
    public async Task Insert_with_client_generated_GUID_key()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        Guid afterSave;
        using (var context = new BlogContextClientGuidKey(testStore.Name, OnModelCreating, GuidSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            var blog = context.Add(
                new GuidBlog
                {
                    Id = GuidSentinel,
                    Name = "One Unicorn",
                    NotId = GuidSentinel
                }).Entity;

            var beforeSave = blog.Id;
            var beforeSaveNotId = blog.NotId;

            Assert.NotEqual(default, beforeSave);
            Assert.NotEqual(default, beforeSaveNotId);

            context.SaveChanges();

            afterSave = blog.Id;
            var afterSaveNotId = blog.NotId;

            Assert.Equal(beforeSave, afterSave);
            Assert.Equal(beforeSaveNotId, afterSaveNotId);
        }

        using (var context = new BlogContextClientGuidKey(testStore.Name, OnModelCreating, GuidSentinel))
        {
            Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
        }
    }

    public class BlogContextClientGuidKey(string databaseName, Action<ModelBuilder> modelBuilder, Guid sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly Guid _sentinel = sentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GuidBlog>(
                eb =>
                {
                    eb.HasAlternateKey(e => e.NotId);
                    eb.Property(e => e.NotId).ValueGeneratedOnAdd().HasSentinel(_sentinel);
                    eb.Property(e => e.Id).HasSentinel(_sentinel);
                });
        }
    }

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.IsNotSqlAzure)]
    public async Task Insert_with_ValueGeneratedOnAdd_GUID_nonkey_property_throws()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using var context = new BlogContextClientGuidNonKey(testStore.Name, OnModelCreating, GuidSentinel);
        context.Database.EnsureCreatedResiliently();

        var blog = context.Add(
            new GuidBlog
            {
                Id = GuidSentinel,
                Name = "One Unicorn",
                NotId = GuidSentinel
            }).Entity;

        Assert.Equal(GuidSentinel, blog.NotId);

        // No value set on a required column
        var updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        Assert.Single(updateException.Entries);
    }

    public class BlogContextClientGuidNonKey(string databaseName, Action<ModelBuilder> modelBuilder, Guid sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly Guid _sentinel = sentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<GuidBlog>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(_sentinel);
                    b.Property(e => e.NotId).ValueGeneratedOnAdd().HasSentinel(_sentinel);
                });
        }
    }

    [ConditionalFact]
    public async Task Insert_with_server_generated_GUID_key()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        Guid afterSave;
        using (var context = new BlogContextServerGuidKey(testStore.Name, OnModelCreating, GuidSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            var blog = context.Add(
                new GuidBlog
                {
                    Id = GuidSentinel,
                    Name = "One Unicorn",
                    NotId = GuidSentinel
                }).Entity;

            var beforeSave = blog.Id;
            var beforeSaveNotId = blog.NotId;

            Assert.Equal(GuidSentinel, beforeSave);
            Assert.Equal(GuidSentinel, beforeSaveNotId);

            context.SaveChanges();

            afterSave = blog.Id;
            var afterSaveNotId = blog.NotId;

            Assert.NotEqual(GuidSentinel, afterSave);
            Assert.NotEqual(GuidSentinel, afterSaveNotId);
            Assert.NotEqual(beforeSave, afterSave);
            Assert.NotEqual(beforeSaveNotId, afterSaveNotId);
        }

        using (var context = new BlogContextServerGuidKey(testStore.Name, OnModelCreating, GuidSentinel))
        {
            Assert.Equal(afterSave, context.GuidBlogs.Single().Id);
        }
    }

    public class BlogContextServerGuidKey(string databaseName, Action<ModelBuilder> modelBuilder, Guid sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly Guid _sentinel = sentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<GuidBlog>(
                    eb =>
                    {
                        eb.Property(e => e.Id).HasDefaultValueSql("newsequentialid()").HasSentinel(_sentinel);
                        eb.Property(e => e.NotId).HasDefaultValueSql("newsequentialid()").HasSentinel(_sentinel);
                    });
        }
    }

    // Negative cases
    [ConditionalFact]
    public async Task Insert_with_explicit_non_default_keys_by_default()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using var context = new BlogContext(testStore.Name, OnModelCreating);
        context.Database.EnsureCreatedResiliently();

        context.AddRange(
            new Blog { Id = 1, Name = "One Unicorn" }, new Blog { Id = 2, Name = "Two Unicorns" });

        // DbUpdateException : An error occurred while updating the entries. See the
        // inner exception for details.
        // SqlException : Cannot insert explicit value for identity column in table
        // 'Blog' when IDENTITY_INSERT is set to OFF.
        context.Database.CreateExecutionStrategy().Execute(context, c => Assert.Throws<DbUpdateException>(() => c.SaveChanges()));
    }

    [ConditionalFact]
    public async Task Insert_with_explicit_default_keys()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using var context = new BlogContext(testStore.Name, OnModelCreating);
        context.Database.EnsureCreatedResiliently();

        context.AddRange(
            new Blog { Id = IntSentinel, Name = "One Unicorn" }, new Blog { Id = 1, Name = "Two Unicorns" });

        // DbUpdateException : An error occurred while updating the entries. See the
        // inner exception for details.
        // SqlException : Cannot insert explicit value for identity column in table
        // 'Blog' when IDENTITY_INSERT is set to OFF.
        var updateException = Assert.Throws<DbUpdateException>(() => context.SaveChanges());
        Assert.Single(updateException.Entries);
    }

    public class BlogContext(string databaseName, Action<ModelBuilder> modelBuilder) : ContextBase(databaseName, modelBuilder);

    [ConditionalFact]
    public async Task Insert_with_implicit_default_keys()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextSpecifyKeysUsingDefault(testStore.Name, OnModelCreating, IntSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.AddRange(
                new Blog { Id = 0, Name = "One Unicorn" }, new Blog { Id = 667, Name = "Two Unicorns" });

            context.SaveChanges();
        }

        using (var context = new BlogContextSpecifyKeysUsingDefault(testStore.Name, OnModelCreating, IntSentinel))
        {
            var blogs = context.Blogs.OrderBy(e => e.Id).ToList();

            Assert.Equal(0, blogs[0].Id);
            Assert.Equal(667, blogs[1].Id);
        }
    }

    public class BlogContextSpecifyKeysUsingDefault(string databaseName, Action<ModelBuilder> modelBuilder, int sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly int _sentinel = sentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .ValueGeneratedNever()
                .HasSentinel(_sentinel);
        }
    }

    [ConditionalFact]
    public async Task Insert_explicit_value_throws_when_readonly_sequence_before_save()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using var context = new BlogContextReadOnlySequenceKeyColumnWithDefaultValue(testStore.Name, OnModelCreating, IntSentinel);
        context.Database.EnsureCreatedResiliently();

        context.AddRange(
            new Blog { Id = 1, Name = "One Unicorn" }, new Blog { Id = IntSentinel, Name = "Two Unicorns" });

        // The property 'Id' on entity type 'Blog' is defined to be read-only before it is
        // saved, but its value has been set to something other than a temporary or default value.
        Assert.Equal(
            CoreStrings.PropertyReadOnlyBeforeSave("Id", "Blog"),
            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
    }

    public class BlogContextReadOnlySequenceKeyColumnWithDefaultValue(string databaseName, Action<ModelBuilder> modelBuilder, int sentinel)
        : ContextBase(databaseName, modelBuilder)
    {
        private readonly int _sentinel = sentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.HasSequence("MySequence");

            var property = modelBuilder
                .Entity<Blog>()
                .Property(e => e.Id)
                .HasDefaultValueSql("next value for MySequence")
                .HasSentinel(_sentinel);

            property.Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Throw);
        }
    }

    [ConditionalFact]
    public async Task Insert_explicit_value_throws_when_readonly_before_save()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using var context = new BlogContextNonKeyReadOnlyDefaultValue(testStore.Name, OnModelCreating, IntSentinel, DateTimeSentinel);
        context.Database.EnsureCreatedResiliently();

        context.AddRange(
            new Blog
            {
                Id = IntSentinel,
                Name = "One Unicorn",
                CreatedOn = DateTimeSentinel
            },
            new Blog
            {
                Id = IntSentinel,
                Name = "Two Unicorns",
                CreatedOn = new DateTime(1969, 8, 3, 0, 10, 0)
            });

        // The property 'CreatedOn' on entity type 'Blog' is defined to be read-only before it is
        // saved, but its value has been set to something other than a temporary or default value.
        Assert.Equal(
            CoreStrings.PropertyReadOnlyBeforeSave("CreatedOn", "Blog"),
            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
    }

    [ConditionalFact]
    public async Task Insert_explicit_value_into_computed_column()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel);
        context.Database.EnsureCreatedResiliently();

        context.Add(
            new FullNameBlog
            {
                Id = IntSentinel,
                FirstName = "One",
                LastName = "Unicorn",
                FullName = "Gerald"
            });

        // The property 'FullName' on entity type 'FullNameBlog' is defined to be read-only before it is
        // saved, but its value has been set to something other than a temporary or default value.
        Assert.Equal(
            CoreStrings.PropertyReadOnlyBeforeSave("FullName", "FullNameBlog"),
            Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
    }

    [ConditionalFact]
    public async Task Update_explicit_value_in_computed_column()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using (var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            context.Database.EnsureCreatedResiliently();

            context.Add(
                new FullNameBlog
                {
                    Id = IntSentinel,
                    FirstName = "One",
                    LastName = "Unicorn",
                    FullName = StringSentinel
                });

            context.SaveChanges();
        }

        using (var context = new BlogContextComputedColumn(testStore.Name, OnModelCreating, IntSentinel, StringSentinel))
        {
            var blog = context.FullNameBlogs.Single();

            blog.FullName = "The Gorilla";

            // The property 'FullName' on entity type 'FullNameBlog' is defined to be read-only after it has been saved,
            // but its value has been modified or marked as modified.
            Assert.Equal(
                CoreStrings.PropertyReadOnlyAfterSave("FullName", "FullNameBlog"),
                Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
        }
    }

    // Concurrency
    [ConditionalFact]
    public async Task Resolve_concurrency()
    {
        using var testStore = await SqlServerTestStore.CreateInitializedAsync(DatabaseName);
        using var context = new BlogContextConcurrencyWithRowversion(testStore.Name, OnModelCreating, IntSentinel, TimestampSentinel);
        context.Database.EnsureCreatedResiliently();

        var blog = context.Add(
            new ConcurrentBlog
            {
                Id = IntSentinel,
                Name = "One Unicorn",
                Timestamp = TimestampSentinel
            }).Entity;

        context.SaveChanges();

        using var innerContext = new BlogContextConcurrencyWithRowversion(testStore.Name, OnModelCreating, IntSentinel, TimestampSentinel);
        var updatedBlog = innerContext.ConcurrentBlogs.Single();
        updatedBlog.Name = "One Pegasus";
        innerContext.SaveChanges();
        var currentTimestamp = updatedBlog.Timestamp.ToArray();

        try
        {
            blog.Name = "One Earth Pony";
            context.SaveChanges();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Update original values (and optionally any current values)
            // Would normally do this with just one method call
            context.Entry(blog).Property(e => e.Id).OriginalValue = updatedBlog.Id;
            context.Entry(blog).Property(e => e.Name).OriginalValue = updatedBlog.Name;
            context.Entry(blog).Property(e => e.Timestamp).OriginalValue = updatedBlog.Timestamp;

            context.SaveChanges();

            Assert.NotEqual(blog.Timestamp, currentTimestamp);
        }
    }

    public class BlogContextConcurrencyWithRowversion(
        string databaseName,
        Action<ModelBuilder> modelBuilder,
        int intSentinel,
        byte[] timestampSentinel) : ContextBase(databaseName, modelBuilder)
    {
        private readonly int _intSentinel = intSentinel;
        private readonly byte[] _timestampSentinel = timestampSentinel;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ConcurrentBlog>(
                b =>
                {
                    b.Property(e => e.Id).HasSentinel(_intSentinel);
                    b.Property(e => e.Timestamp)
                        .ValueGeneratedOnAddOrUpdate()
                        .IsConcurrencyToken()
                        .HasSentinel(_timestampSentinel);
                });
        }
    }

    protected Blog CreateBlog(string name)
        => new()
        {
            Id = IntSentinel,
            Name = name,
            CreatedOn = DateTimeSentinel,
            NeedsConverter = NeedsConverterSentinel,
            OtherId = NullableIntSentinel
        };

    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
        public NeedsConverter NeedsConverter { get; set; }
        public int? OtherId { get; set; }
    }

    protected BlogWithSpatial CreateBlogWithSpatial(string name)
        => new()
        {
            Id = IntSentinel,
            Name = name,
            GeometryCollection = GeometryCollectionSentinel,
        };

    public class BlogWithSpatial
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public GeometryCollection GeometryCollection { get; set; }
    }

    public class NeedsConverter(int value)
    {
        public int Value { get; } = value;

        public override bool Equals(object obj)
            => throw new InvalidOperationException();

        public override int GetHashCode()
            => throw new InvalidOperationException();
    }

    public class NullableKeyBlog
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedOn { get; set; }
    }

    public class FullNameBlog
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
    }

    public class GuidBlog
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid NotId { get; set; }
    }

    public class ConcurrentBlog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public byte[] Timestamp { get; set; }
    }

    protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        => modelBuilder.Entity<Blog>()
            .Property(e => e.NeedsConverter)
            .HasConversion(
                v => v.Value,
                v => new NeedsConverter(v),
                new ValueComparer<NeedsConverter>(
                    (l, r) => (l == null && r == null) || (l != null && r != null && l.Value == r.Value),
                    v => v.Value.GetHashCode(),
                    v => new NeedsConverter(v.Value)))
            .HasDefaultValue(new NeedsConverter(999));

    public abstract class ContextBase : DbContext
    {
        private readonly string _databaseName;
        private readonly Action<ModelBuilder> _modelBuilder;

        protected ContextBase(string databaseName, Action<ModelBuilder> modelBuilder)
        {
            _databaseName = databaseName;
            _modelBuilder = modelBuilder;
        }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<BlogWithSpatial> SpatialBlogs { get; set; }
        public DbSet<NullableKeyBlog> NullableKeyBlogs { get; set; }
        public DbSet<FullNameBlog> FullNameBlogs { get; set; }
        public DbSet<GuidBlog> GuidBlogs { get; set; }
        public DbSet<ConcurrentBlog> ConcurrentBlogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => _modelBuilder(modelBuilder);

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .EnableServiceProviderCaching(false)
                .UseSqlServer(
                    SqlServerTestStore.CreateConnectionString(_databaseName),
                    b => b.UseNetTopologySuite().ApplyConfiguration());
    }

    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };
}
