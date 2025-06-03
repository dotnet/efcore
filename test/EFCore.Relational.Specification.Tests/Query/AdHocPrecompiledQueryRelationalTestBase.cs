// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Query.Internal;
using static Microsoft.EntityFrameworkCore.TestUtilities.PrecompiledQueryTestHelpers;

namespace Microsoft.EntityFrameworkCore.Query;

[Collection("PrecompiledQuery")]
public abstract class AdHocPrecompiledQueryRelationalTestBase: NonSharedModelTestBase, IClassFixture<NonSharedFixture>
{
    public AdHocPrecompiledQueryRelationalTestBase(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
    {
        TestOutputHelper = testOutputHelper;
    }

    [ConditionalFact]
    public virtual async Task Index_no_evaluatability()
    {
        var contextFactory = await InitializeAsync<JsonContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.JsonContext(dbContextOptions);
await context.Database.BeginTransactionAsync();

var blogs = context.JsonEntities.Where(b => b.IntList[b.Id] == 2).ToList();
""",
            typeof(JsonContext),
            options);
    }

    [ConditionalFact]
    public virtual async Task Index_with_captured_variable()
    {
        var contextFactory = await InitializeAsync<JsonContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.JsonContext(dbContextOptions);
await context.Database.BeginTransactionAsync();

var id = 1;
var blogs = context.JsonEntities.Where(b => b.IntList[id] == 2).ToList();
""",
            typeof(JsonContext),
            options);
    }

    [ConditionalFact]
    public virtual async Task JsonScalar()
    {
        var contextFactory = await InitializeAsync<JsonContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.JsonContext(dbContextOptions);
await context.Database.BeginTransactionAsync();

_ = context.JsonEntities.Where(b => b.JsonThing.StringProperty == "foo").ToList();
""",
            typeof(JsonContext),
            options);
    }

    public class JsonContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<JsonEntity> JsonEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<JsonEntity>().OwnsOne(j => j.JsonThing, n => n.ToJson());
    }

    public class JsonEntity
    {
        public int Id { get; set; }
        public List<int> IntList { get; set; } = null!;
        public JsonThing JsonThing { get; set; } = null!;
    }

    public class JsonThing
    {
        public string StringProperty { get; set; } = null!;
    }

    [ConditionalFact]
    public virtual async Task Materialize_non_public()
    {
        var contextFactory = await InitializeAsync<NonPublicContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.NonPublicContext(dbContextOptions);

var nonPublicEntity = (AdHocPrecompiledQueryRelationalTestBase.NonPublicEntity)Activator.CreateInstance(typeof(AdHocPrecompiledQueryRelationalTestBase.NonPublicEntity), nonPublic: true);
nonPublicEntity.PrivateFieldExposer = 8;
nonPublicEntity.PrivatePropertyExposer = 9;
nonPublicEntity.PrivateAutoPropertyExposer = 10;
context.NonPublicEntities.Add(nonPublicEntity);
await context.SaveChangesAsync();

context.ChangeTracker.Clear();

var e = await context.NonPublicEntities.SingleAsync();
Assert.Equal(8, e.PrivateFieldExposer);
Assert.Equal(9, e.PrivatePropertyExposer);
Assert.Equal(10, e.PrivateAutoPropertyExposer);
""",
            typeof(NonPublicContext),
            options,
            interceptorCodeAsserter: code =>
            {
                Assert.Contains("""[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<Id>k__BackingField")]""", code);
                Assert.Contains("""[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "<PrivateAutoProperty>k__BackingField")]""", code);
                Assert.Contains("""[UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_privateField")]""", code);
                Assert.Contains("""[UnsafeAccessor(UnsafeAccessorKind.Constructor)]""", code);
                Assert.Contains("""[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_PrivateProperty")]""", code);

                Assert.Contains("var instance = UnsafeAccessor_Microsoft_EntityFrameworkCore_Query_NonPublicEntity_Ctor();", code);
                Assert.Contains("UnsafeAccessor_Microsoft_EntityFrameworkCore_Query_NonPublicEntity_Id_Set(instance) =", code);
                Assert.Contains(
                    "UnsafeAccessor_Microsoft_EntityFrameworkCore_Query_NonPublicEntity_PrivateAutoProperty_Set(instance) =", code);
                Assert.Contains("UnsafeAccessor_Microsoft_EntityFrameworkCore_Query_NonPublicEntity_set_PrivateProperty(instance,", code);
                Assert.Contains("UnsafeAccessor_Microsoft_EntityFrameworkCore_Query_NonPublicEntity__privateField_Set(instance) =", code);
            });
    }

    public class NonPublicContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<NonPublicEntity> NonPublicEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<NonPublicEntity>(
                b =>
                {
                    b.Property<int?>("_privateField");
                    b.Property<int?>("PrivateProperty");
                    b.Property<int?>("PrivateAutoProperty");
                    b.Ignore(b => b.PrivateFieldExposer);
                    b.Ignore(b => b.PrivatePropertyExposer);
                    b.Ignore(b => b.PrivateAutoPropertyExposer);
                });
    }

#pragma warning disable CS0169
#pragma warning disable CS0649
    public class NonPublicEntity
    {
        private NonPublicEntity()
        {
        }

        public int Id { get; set; }

        private int? _privateField;

        // ReSharper disable once ConvertToAutoProperty
        private int? PrivateProperty
        {
            get => _privatePropertyBackingField;
            set => _privatePropertyBackingField = value;
        }

        private int? _privatePropertyBackingField;

        private int? PrivateAutoProperty { get; set; }

        // ReSharper disable once ConvertToAutoProperty
        public int? PrivateFieldExposer
        {
            get => _privateField;
            set => _privateField = value;
        }

        public int? PrivatePropertyExposer
        {
            get => PrivateProperty;
            set => PrivateProperty = value;
        }

        public int? PrivateAutoPropertyExposer
        {
            get => PrivateAutoProperty;
            set => PrivateAutoProperty = value;
        }
    }
#pragma warning restore CS0649
#pragma warning restore CS0169

//     [ConditionalFact]
//     public virtual Task JsonScalar()
//         => Test(
//             // TODO: Remove Select() to Id after JSON is supported in materialization
//             """_ = context.Blogs.Where(b => b.JsonThing.SomeProperty == "foo").Select(b => b.Id).ToList();""",
//             modelSourceCode: providerOptions => $$"""
// public class BlogContext : DbContext
// {
//     public DbSet<Blog> Blogs { get; set; }
//
//     protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//         => optionsBuilder
//             {{providerOptions}}
//             .ReplaceService<IQueryCompiler, Microsoft.EntityFrameworkCore.Query.NonCompilingQueryCompiler>();
//
//     protected override void OnModelCreating(ModelBuilder modelBuilder)
//         => modelBuilder.Entity<Blog>().OwnsOne(b => b.JsonThing, n => n.ToJson());
// }
//
// public class Blog
// {
//     public int Id { get; set; }
//     public JsonThing JsonThing { get; set; }
// }
//
// public class JsonThing
// {
//     public string SomeProperty { get; set; }
// }
// """);

    [ConditionalFact]
    public virtual async Task Projecting_property_requiring_converter_with_closure_is_not_supported()
    {
        var contextFactory = await InitializeAsync<PrecompiledContext34760>();
        var options = contextFactory.GetOptions();

        await Test(
"""
await using var context = new AdHocPrecompiledQueryRelationalTestBase.PrecompiledContext34760(dbContextOptions);
var publishDates = await context.Books.Select(x => x.PublishDate).ToListAsync();
""",
            typeof(PrecompiledContext34760),
            options,
            precompilationErrorAsserter: errors
                => Assert.StartsWith(
                    "Encountered a constant of unsupported type 'MyDatetimeConverter'. Only primitive constant nodes are supported.",
                    errors.Single().Exception.Message));
    }

    [ConditionalFact]
    public virtual async Task Projecting_expression_requiring_converter_without_closure_works()
    {
        var contextFactory = await InitializeAsync<PrecompiledContext34760>();
        var options = contextFactory.GetOptions();

        await Test(
"""
await using var context = new AdHocPrecompiledQueryRelationalTestBase.PrecompiledContext34760(dbContextOptions);
var audiobookDates = await context.Books.Select(x => x.AudiobookDate).ToListAsync();
""",
            typeof(PrecompiledContext34760),
            options);
    }

    [ConditionalFact]
    public virtual async Task Projecting_entity_with_property_requiring_converter_with_closure_works()
    {
        var contextFactory = await InitializeAsync<PrecompiledContext34760>();
        var options = contextFactory.GetOptions();

        await Test(
"""
await using var context = new AdHocPrecompiledQueryRelationalTestBase.PrecompiledContext34760(dbContextOptions);
var books = await context.Books.ToListAsync();
""",
            typeof(PrecompiledContext34760),
            options);
    }

    public class PrecompiledContext34760(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Book> Books { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Book>().Property(e => e.PublishDate).HasConversion(new MyDateTimeValueConverterWithClosure(new MyDatetimeConverter()));
            modelBuilder.Entity<Book>().Property(e => e.AudiobookDate).HasConversion(new MyDateTimeValueConverterWithoutClosure());
        }

        public Task SeedAsync()
        {
            AddRange(
                new Book
                {
                    Id = 1,
                    Name = "The Blade Itself",
                    PublishDate = new DateTime(2006, 5, 4, 11, 59, 59),
                    AudiobookDate = new DateTime(2015, 9, 8, 23, 59, 59)
                },
                new Book
                {
                    Id = 2,
                    Name = "Red Rising",
                    PublishDate = new DateTime(2014, 1, 27, 23, 59, 59),
                    AudiobookDate = new DateTime(2014, 1, 27, 23, 59, 59),
                });

            return SaveChangesAsync();
        }

        public class Book
        {
            public int Id { get; set; }
            public string? Name { get; set; }

            public virtual DateTime PublishDate { get; set; }
            public virtual DateTime AudiobookDate { get; set; }
        }

        public class MyDateTimeValueConverterWithClosure : ValueConverter<DateTime, DateTime>
        {
            public MyDateTimeValueConverterWithClosure(MyDatetimeConverter myDatetimeConverter)
                : base(
                    x => myDatetimeConverter.Normalize(x),
                    x => myDatetimeConverter.Normalize(x))
            {
            }
        }

        public class MyDateTimeValueConverterWithoutClosure : ValueConverter<DateTime, DateTime>
        {
            public MyDateTimeValueConverterWithoutClosure()
                : base(
                    x => new MyDatetimeConverter().Normalize(x),
                    x => new MyDatetimeConverter().Normalize(x))
            {
            }
        }

        public class MyDatetimeConverter
        {
            public virtual DateTime Normalize(DateTime dateTime)
                => dateTime.Date;
        }
    }

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    protected void AssertSql(params string[] expected)
        => TestSqlLoggerFactory.AssertBaseline(expected);

    protected virtual Task Test(
        string sourceCode,
        Type dbContextType,
        DbContextOptions dbContextOptions,
        Action<string>? interceptorCodeAsserter = null,
        Action<List<PrecompiledQueryCodeGenerator.QueryPrecompilationError>>? precompilationErrorAsserter = null,
        [CallerMemberName] string callerName = "")
        => PrecompiledQueryTestHelpers.Test(
            sourceCode, dbContextOptions, dbContextType, interceptorCodeAsserter, precompilationErrorAsserter, TestOutputHelper!,
            AlwaysPrintGeneratedSources,
            callerName);

    protected virtual bool AlwaysPrintGeneratedSources
        => false;

    protected abstract PrecompiledQueryTestHelpers PrecompiledQueryTestHelpers { get; }

    protected override IServiceCollection AddServices(IServiceCollection serviceCollection)
        => base.AddServices(serviceCollection)
            .AddScoped<IQueryCompiler, NonCompilingQueryCompiler>();

    protected override string StoreName
        => "AdHocPrecompiledQueryTest";
}
