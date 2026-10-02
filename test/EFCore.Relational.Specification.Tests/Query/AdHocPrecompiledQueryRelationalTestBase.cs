// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query.Internal;
using static Microsoft.EntityFrameworkCore.TestUtilities.PrecompiledQueryTestHelpers;

namespace Microsoft.EntityFrameworkCore.Query;

[Collection("PrecompiledQuery")]
public abstract class AdHocPrecompiledQueryRelationalTestBase : NonSharedModelTestBase, IClassFixture<NonSharedFixture>
{
    public AdHocPrecompiledQueryRelationalTestBase(NonSharedFixture fixture, ITestOutputHelper testOutputHelper)
        : base(fixture)
        => TestOutputHelper = testOutputHelper;

    [Fact]
    public virtual async Task Index_no_evaluatability()
    {
        var contextFactory = await InitializeNonSharedTest<JsonContext>();
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

    [Fact]
    public virtual async Task Index_with_captured_variable()
    {
        var contextFactory = await InitializeNonSharedTest<JsonContext>();
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

    [Fact]
    public virtual async Task JsonScalar()
    {
        var contextFactory = await InitializeNonSharedTest<JsonContext>();
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

    [Fact]
    public virtual async Task Materialize_non_public()
    {
        var contextFactory = await InitializeNonSharedTest<NonPublicContext>();
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
            => modelBuilder.Entity<NonPublicEntity>(b =>
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

//     [Fact]
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

    [Fact]
    public virtual async Task Projecting_property_requiring_converter_with_closure_is_not_supported()
    {
        var contextFactory = await InitializeNonSharedTest<PrecompiledContext34760>();
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

    [Fact]
    public virtual async Task Projecting_expression_requiring_converter_without_closure_works()
    {
        var contextFactory = await InitializeNonSharedTest<PrecompiledContext34760>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.PrecompiledContext34760(dbContextOptions);
var audiobookDates = await context.Books.Select(x => x.AudiobookDate).ToListAsync();
""",
            typeof(PrecompiledContext34760),
            options);
    }

    [Fact]
    public virtual async Task Projecting_entity_with_property_requiring_converter_with_closure_works()
    {
        var contextFactory = await InitializeNonSharedTest<PrecompiledContext34760>();
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
            modelBuilder.Entity<Book>().Property(e => e.PublishDate)
                .HasConversion(new MyDateTimeValueConverterWithClosure(new MyDatetimeConverter()));
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

    #region Invalid runtime constant name

    [Fact]
    public virtual async Task Invalid_identifier_json_property_name()
    {
        var contextFactory = await InitializeNonSharedTest<InvalidNameContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.InvalidNameContext(dbContextOptions);
var books = await context.Entities.ToListAsync();
""",
            typeof(InvalidNameContext),
            options,
            interceptorCodeAsserter: (code) =>
            {
                Assert.Contains("_1_NOT_VALID_Bytes = ", code);
                Assert.Contains("_1_NOT_VALID_Bytes0 = ", code);
            });
    }

    public class InvalidNameContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<InvalidNameEntity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<InvalidNameEntity>().ComplexProperty(x => x.Nested, b =>
            {
                b.ToJson();
                b.Property(x => x.Name).HasJsonPropertyName("1!NOT VALID;");
                b.Property(x => x.Name2).HasJsonPropertyName("1-NOT VALID!");
            });
    }

    public class InvalidNameEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public InvalidNameNestedEntity Nested { get; set; } = new();
    }

    public class InvalidNameNestedEntity
    {
        public string Name { get; set; } = "";
        public string Name2 { get; set; } = "";
    }

    [Fact]
    public virtual async Task Invalid_identifier_shadow_property_name()
    {
        var contextFactory = await InitializeNonSharedTest<InvalidShadowNameContext>(
            onConfiguring: o => o.ConfigureWarnings(w => w.Ignore(CoreEventId.ShadowPropertyNameNotValidIdentifierWarning)));
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.InvalidShadowNameContext(dbContextOptions);
var entities = await context.Entities.ToListAsync();
""",
            typeof(InvalidShadowNameContext),
            options);
    }

    public class InvalidShadowNameContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<InvalidShadowNameEntity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<InvalidShadowNameEntity>()
                .Property<string>("NOT VALID !!!1").HasConversion<int>(x => 0, x => "");
    }

    public class InvalidShadowNameEntity
    {
        public Guid Id { get; set; }
    }

#pragma warning disable EF9100
    [Fact]
    public virtual async Task Query_reusing_a_runtime_constant_of_a_query_that_failed_to_precompile()
    {
        var contextFactory = await InitializeNonSharedTest<SharedRuntimeConstantContext>(
            addServices: s => s.AddSingleton<ILiftableConstantFactory, PoisonFirstLiftableConstantFactory>());
        var options = contextFactory.GetOptions();

        // Both queries read the same JSON property, whose name is emitted as a runtime constant, so the second one depends on
        // state the first one registered before it failed.
        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.SharedRuntimeConstantContext(dbContextOptions);
if (Environment.GetEnvironmentVariable("EF_TEST_NEVER_SET") is not null)
{
    var first = await context.Entities.ToListAsync();
}

var second = await context.Entities.OrderBy(e => e.Id).ToListAsync();
""",
            typeof(SharedRuntimeConstantContext),
            options,
            precompilationErrorAsserter: errors => Assert.Single(errors));
    }

    [Fact]
    public virtual async Task Runtime_constants_differing_only_by_a_formatting_character_get_distinct_fields()
    {
        var contextFactory = await InitializeNonSharedTest<FormattingCharacterContext>();
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.FormattingCharacterContext(dbContextOptions);
var entities = await context.Entities.ToListAsync();
""",
            typeof(FormattingCharacterContext),
            options,
            interceptorCodeAsserter: code =>
            {
                Assert.Contains("_NameBytes", code);
                Assert.Contains("_Na_meBytes", code);
            });
    }

    // Two JSON property names that differ only by U+200C, a formatting character, which C# ignores when comparing identifiers:
    // emitted as they are, the two runtime constant fields would be one duplicate declaration
    public class FormattingCharacterContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<FormattingCharacterEntity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<FormattingCharacterEntity>().ComplexProperty(
                x => x.Nested, b =>
                {
                    b.ToJson();
                    b.Property(x => x.Name).HasJsonPropertyName("Name");
                    b.Property(x => x.Other).HasJsonPropertyName("Na\u200Cme");
                });
    }

    public class FormattingCharacterEntity
    {
        public Guid Id { get; set; }
        public FormattingCharacterNested Nested { get; set; } = new();
    }

    public class FormattingCharacterNested
    {
        public string Name { get; set; } = "";
        public string Other { get; set; } = "";
    }

    [Fact]
    public virtual async Task Liftable_constant_named_like_a_runtime_constant_field()
    {
        var contextFactory = await InitializeNonSharedTest<SharedRuntimeConstantContext>(
            addServices: s => s.AddSingleton<ILiftableConstantFactory, RuntimeConstantFieldLiftableConstantFactory>());
        var options = contextFactory.GetOptions();

        // Lifted constants start lowercase and runtime constant fields start with an underscore, but a lifted name can start with an
        // underscore too, so the field's name is the one place the two schemes can meet
        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.SharedRuntimeConstantContext(dbContextOptions);
var entities = await context.Entities.ToListAsync();
""",
            typeof(SharedRuntimeConstantContext),
            options,
            interceptorCodeAsserter: code =>
            {
                Assert.Contains("_NameBytes = ", code);
                Assert.Contains("var _NameBytes0 = ", code);
            });
    }

    public class RuntimeConstantFieldLiftableConstantFactory(LiftableConstantExpressionDependencies dependencies)
        : RenamingLiftableConstantFactory(dependencies)
    {
        protected override string Name
            => "_NameBytes";
    }

    [Fact]
    public virtual async Task Runtime_constant_shared_by_two_queries_is_emitted_once()
    {
        var contextFactory = await InitializeNonSharedTest<SharedRuntimeConstantContext>();
        var options = contextFactory.GetOptions();

        // Each query's shaper evaluates the JSON property name into a byte array of its own, so the two constants are equal by
        // initializer but not by value, and must still share one field.
        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.SharedRuntimeConstantContext(dbContextOptions);
var first = await context.Entities.ToListAsync();
var second = await context.Entities.OrderBy(e => e.Id).ToListAsync();
""",
            typeof(SharedRuntimeConstantContext),
            options,
            interceptorCodeAsserter: code =>
            {
                Assert.Equal(1, Regex.Matches(code, @"\bprivate\s+static\s+readonly\b[^;=]*\b_NameBytes\s*=").Count);
                Assert.DoesNotContain("_NameBytes0", code);
            });
    }

    public class SharedRuntimeConstantContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<SharedRuntimeConstantEntity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<SharedRuntimeConstantEntity>()
                .ComplexProperty(x => x.Nested, b => b.ToJson());
    }

    public class SharedRuntimeConstantEntity
    {
        public Guid Id { get; set; }
        public SharedRuntimeConstantNested Nested { get; set; } = new();
    }

    public class SharedRuntimeConstantNested
    {
        public string Name { get; set; } = "";
    }

    // Poisons only the first liftable constant, so the first query fails while generating its executor and the second does not
    public class PoisonFirstLiftableConstantFactory(LiftableConstantExpressionDependencies dependencies)
        : LiftableConstantFactory(dependencies)
    {
        private bool _poisoned;

        public override Expression CreateLiftableConstant(
            object? originalValue,
            Expression<Func<MaterializerLiftableConstantContext, object>> resolverExpression,
            string variableName,
            Type type)
        {
            if (!_poisoned)
            {
                _poisoned = true;
                resolverExpression = Expression.Lambda<Func<MaterializerLiftableConstantContext, object>>(
                    Expression.Parameter(typeof(object)), resolverExpression.Parameters);
            }

            return base.CreateLiftableConstant(originalValue, resolverExpression, variableName, type);
        }
    }

    [Fact]
    public virtual async Task Query_that_fails_to_precompile_leaves_the_other_queries_compilable()
    {
        var contextFactory = await InitializeNonSharedTest<PartialOutputContext>(
            addServices: s => s.AddSingleton<ILiftableConstantFactory, UntranslatableLiftableConstantFactory>());
        var options = contextFactory.GetOptions();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.PartialOutputContext(dbContextOptions);
if (Environment.GetEnvironmentVariable("EF_TEST_NEVER_SET") is not null)
{
    var firsts = await context.Firsts.ToListAsync();
}

var seconds = await context.Seconds.ToListAsync();
""",
            typeof(PartialOutputContext),
            options,
            interceptorCodeAsserter: code => Assert.Contains("UnsafeAccessor", code),
            precompilationErrorAsserter: errors => Assert.Single(errors));
    }

    [Fact]
    public virtual async Task Query_that_fails_to_precompile_between_two_that_succeed_leaves_both_compilable()
    {
        var contextFactory = await InitializeNonSharedTest<PartialOutputContext>(
            addServices: s => s.AddSingleton<ILiftableConstantFactory, UntranslatableLiftableConstantFactory>());
        var options = contextFactory.GetOptions();

        // Code is added to the file before and after the failed query, so a splice that went wrong would corrupt either neighbour
        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.PartialOutputContext(dbContextOptions);
var seconds = await context.Seconds.ToListAsync();
if (Environment.GetEnvironmentVariable("EF_TEST_NEVER_SET") is not null)
{
    var firsts = await context.Firsts.ToListAsync();
}

var orderedSeconds = await context.Seconds.OrderBy(s => s.Id).ToListAsync();
""",
            typeof(PartialOutputContext),
            options,
            interceptorCodeAsserter: code =>
            {
                Assert.Contains("#region Query1", code);
                Assert.DoesNotContain("#region Query2", code);
                Assert.Contains("#region Query3", code);
            },
            precompilationErrorAsserter: errors => Assert.Single(errors));
    }

    public class PartialOutputContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<PartialOutputFirst> Firsts { get; set; } = null!;
        public DbSet<PartialOutputSecond> Seconds { get; set; } = null!;
    }

    public class PartialOutputFirst
    {
        // A private setter makes the generated code reach this through an unsafe accessor, which the translator caches and
        // re-adds on every later translation, so the failed query's accessor has to stay compilable
        public Guid Id { get; private set; }
    }

    public class PartialOutputSecond
    {
        public Guid Id { get; set; }
    }

    // Gives the first query's entity type a resolver the C# translator cannot emit, so that query fails while generating its
    // executor, after its interceptors have already been written. That is what leaves partial output behind.
    public class UntranslatableLiftableConstantFactory(LiftableConstantExpressionDependencies dependencies)
        : LiftableConstantFactory(dependencies)
    {
        public override Expression CreateLiftableConstant(
            object? originalValue,
            Expression<Func<MaterializerLiftableConstantContext, object>> resolverExpression,
            string variableName,
            Type type)
        {
            if (originalValue is IEntityType entityType && entityType.ClrType == typeof(PartialOutputFirst))
            {
                resolverExpression = Expression.Lambda<Func<MaterializerLiftableConstantContext, object>>(
                    Expression.Parameter(typeof(object)), resolverExpression.Parameters);
            }

            return base.CreateLiftableConstant(originalValue, resolverExpression, variableName, type);
        }
    }

    [Fact]
    public virtual Task Liftable_constant_named_like_a_keyword()
        => TestLiftableConstantName<KeywordLiftableConstantFactory>();

    [Fact]
    public virtual Task Liftable_constant_named_like_the_query_context_parameter()
        => TestLiftableConstantName<QueryContextLiftableConstantFactory>();

    [Fact]
    public virtual Task Liftable_constant_named_like_the_db_context_parameter()
        => TestLiftableConstantName<DbContextLiftableConstantFactory>();

    [Fact]
    public virtual Task Liftable_constant_whose_sanitized_name_is_another_constants_name()
        => TestLiftableConstantName<CollidingLiftableConstantFactory>();

    // Every constant is given the same name, so the name under test is certain to reach the generated file whichever constants
    // the optimizer keeps, and the duplicates exercise uniquification at the same time.
    private async Task TestLiftableConstantName<TFactory>([CallerMemberName] string callerName = "")
        where TFactory : class, ILiftableConstantFactory
    {
        var contextFactory = await InitializeNonSharedTest<KeywordLiftableConstantContext>(
            addServices: s => s.AddSingleton<ILiftableConstantFactory, TFactory>());

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.KeywordLiftableConstantContext(dbContextOptions);
var entities = await context.Entities.ToListAsync();
""",
            typeof(KeywordLiftableConstantContext),
            contextFactory.GetOptions(),
            callerName: callerName);
    }

    public class KeywordLiftableConstantContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<KeywordLiftableConstantEntity> Entities { get; set; } = null!;
    }

    public class KeywordLiftableConstantEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = "";
    }

    // The variable name is whatever the caller passes, and providers call this API, so it can be a C# keyword or one of the
    // identifiers the generated executor declares for itself.
    public abstract class RenamingLiftableConstantFactory(LiftableConstantExpressionDependencies dependencies)
        : LiftableConstantFactory(dependencies)
    {
        protected abstract string Name { get; }

        public override Expression CreateLiftableConstant(
            object? originalValue,
            Expression<Func<MaterializerLiftableConstantContext, object>> resolverExpression,
            string variableName,
            Type type)
            => base.CreateLiftableConstant(originalValue, resolverExpression, Name, type);
    }

    public class KeywordLiftableConstantFactory(LiftableConstantExpressionDependencies dependencies)
        : RenamingLiftableConstantFactory(dependencies)
    {
        protected override string Name
            => "Class";
    }

    public class QueryContextLiftableConstantFactory(LiftableConstantExpressionDependencies dependencies)
        : RenamingLiftableConstantFactory(dependencies)
    {
        protected override string Name
            => "queryContext";
    }

    public class DbContextLiftableConstantFactory(LiftableConstantExpressionDependencies dependencies)
        : RenamingLiftableConstantFactory(dependencies)
    {
        protected override string Name
            => "DbContext";
    }

    // One name sanitizes into the other, so the two must not end up sharing a declaration
    public class CollidingLiftableConstantFactory(LiftableConstantExpressionDependencies dependencies)
        : LiftableConstantFactory(dependencies)
    {
        public override Expression CreateLiftableConstant(
            object? originalValue,
            Expression<Func<MaterializerLiftableConstantContext, object>> resolverExpression,
            string variableName,
            Type type)
            => base.CreateLiftableConstant(
                originalValue, resolverExpression, originalValue is IEntityType ? "Class" : "_class", type);
    }

    [Fact]
    public virtual Task Runtime_constant_named_like_the_executor_field()
        => TestRuntimeConstantName<ExecutorFieldRuntimeConstantVisitorFactory>();

    [Fact]
    public virtual Task Runtime_constant_named_like_the_interceptors_class()
        => TestRuntimeConstantName<InterceptorsClassRuntimeConstantVisitorFactory>();

    [Fact]
    public virtual Task Runtime_constant_named_like_a_type_the_generated_code_uses()
        => TestRuntimeConstantName<TypeNameRuntimeConstantVisitorFactory>();

    [Fact]
    public virtual Task Runtime_constant_named_like_a_reserved_token_once_prefixed()
        => TestRuntimeConstantName<ReservedTokenRuntimeConstantVisitorFactory>();

    [Fact]
    public virtual Task Runtime_constant_named_like_a_type_with_a_leading_underscore()
        => TestRuntimeConstantName<UnderscoreTypeRuntimeConstantVisitorFactory>();

    [Fact]
    public virtual async Task Shaper_variables_named_like_the_executor_identifiers_are_uniquified()
    {
        var contextFactory = await InitializeNonSharedTest<RuntimeConstantNameContext>(
            addServices: s => s.AddScoped<IShapedQueryCompilingExpressionVisitorFactory, ExecutorNamedVariablesVisitorFactory>());

        // The shaper's variables end up declared inside the executor lambda, which the generated executor method wraps in its own
        // parameters and locals of these names
        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.RuntimeConstantNameContext(dbContextOptions);
var entities = await context.Entities.ToListAsync();
""",
            typeof(RuntimeConstantNameContext),
            contextFactory.GetOptions(),
            interceptorCodeAsserter: code =>
            {
                foreach (var name in ExecutorNamedVariablesVisitorFactory.Names)
                {
                    Assert.DoesNotContain($"var {name} = 1;", code);
                    Assert.Matches($@"\bvar {name}\d+ = 1;", code);
                }
            });
    }

    // A shaper that declares variables under the names the generated executor method uses for its own parameters and locals
    public class ExecutorNamedVariablesVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies)
        : RelationalShapedQueryCompilingExpressionVisitorFactory(dependencies, relationalDependencies)
    {
        public static readonly string[] Names =
        [
            "dbContext", "queryContext", "relationalModel", "relationalTypeMappingSource", "materializerLiftableConstantContext"
        ];

        public override ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
            => new ExecutorNamedVariablesVisitor(Dependencies, RelationalDependencies, queryCompilationContext);

        private sealed class ExecutorNamedVariablesVisitor(
            ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext)
            : RelationalShapedQueryCompilingExpressionVisitor(dependencies, relationalDependencies, queryCompilationContext)
        {
            protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
            {
                var variables = Names.Select(name => Expression.Variable(typeof(int), name)).ToList();

                return Expression.Block(
                    variables,
                    variables.Select(variable => (Expression)Expression.Assign(variable, Expression.Constant(1)))
                        .Append(base.VisitShapedQuery(shapedQueryExpression)));
            }
        }
    }

    // A runtime constant becomes a field of the generated interceptors class, and its name is whatever the shaper that created it
    // chose, so a provider's shaper can hand it the name of a member the generator emits for itself, or of a type the generated
    // code refers to by its simple name.
    private async Task TestRuntimeConstantName<TFactory>([CallerMemberName] string callerName = "")
        where TFactory : class, IShapedQueryCompilingExpressionVisitorFactory
    {
        var contextFactory = await InitializeNonSharedTest<RuntimeConstantNameContext>(
            addServices: s => s.AddScoped<IShapedQueryCompilingExpressionVisitorFactory, TFactory>());

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.RuntimeConstantNameContext(dbContextOptions);
var entities = await context.Entities.ToListAsync();
""",
            typeof(RuntimeConstantNameContext),
            contextFactory.GetOptions(),
            callerName: callerName);
    }

    public class RuntimeConstantNameContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<RuntimeConstantNameEntity> Entities { get; set; } = null!;
    }

    public class RuntimeConstantNameEntity
    {
        public Guid Id { get; set; }
    }

    // Emits a runtime constant under the given name, the way the relational shaper emits a JSON property name
    public abstract class NamingRuntimeConstantVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies)
        : RelationalShapedQueryCompilingExpressionVisitorFactory(dependencies, relationalDependencies)
    {
        protected abstract string Name { get; }

        // The relational shaper's own initializer for a JSON property name
        protected virtual Expression CreateInitializer(string name)
            => Expression.Call(
                Expression.Property(null, typeof(Encoding), nameof(Encoding.UTF8)),
                typeof(Encoding).GetMethod(nameof(Encoding.GetBytes), [typeof(string)])!,
                Expression.Constant(name));

        public override ShapedQueryCompilingExpressionVisitor Create(QueryCompilationContext queryCompilationContext)
            => new NamingRuntimeConstantVisitor(
                Dependencies, RelationalDependencies, queryCompilationContext, Name, CreateInitializer(Name));

        private sealed class NamingRuntimeConstantVisitor(
            ShapedQueryCompilingExpressionVisitorDependencies dependencies,
            RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies,
            QueryCompilationContext queryCompilationContext,
            string name,
            Expression initializer)
            : RelationalShapedQueryCompilingExpressionVisitor(dependencies, relationalDependencies, queryCompilationContext)
        {
            protected override Expression VisitShapedQuery(ShapedQueryExpression shapedQueryExpression)
                => Expression.Block(
                    Expression.Call(typeof(GC).GetMethod(nameof(GC.KeepAlive))!, new RuntimeConstantExpression(name, initializer)),
                    base.VisitShapedQuery(shapedQueryExpression));
        }
    }

    public class ExecutorFieldRuntimeConstantVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies)
        : NamingRuntimeConstantVisitorFactory(dependencies, relationalDependencies)
    {
        protected override string Name
            => "Query1_Executor";
    }

    public class InterceptorsClassRuntimeConstantVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies)
        : NamingRuntimeConstantVisitorFactory(dependencies, relationalDependencies)
    {
        protected override string Name
            => "EntityFrameworkCoreInterceptors";
    }

    // The field's own initializer is Encoding.UTF8.GetBytes(...), as it is for every JSON property name
    public class TypeNameRuntimeConstantVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies)
        : NamingRuntimeConstantVisitorFactory(dependencies, relationalDependencies)
    {
        protected override string Name
            => "Encoding";
    }

    // A valid identifier on its own, but a reserved token once the field prefix is in front of it
    public class ReservedTokenRuntimeConstantVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies)
        : NamingRuntimeConstantVisitorFactory(dependencies, relationalDependencies)
    {
        protected override string Name
            => "_arglist";
    }

    // The prefixed field name is exactly the name of the type the initializer reads a static member from
    public class UnderscoreTypeRuntimeConstantVisitorFactory(
        ShapedQueryCompilingExpressionVisitorDependencies dependencies,
        RelationalShapedQueryCompilingExpressionVisitorDependencies relationalDependencies)
        : NamingRuntimeConstantVisitorFactory(dependencies, relationalDependencies)
    {
        protected override string Name
            => "RuntimeConstantSource";

        protected override Expression CreateInitializer(string name)
            => Expression.Property(null, typeof(_RuntimeConstantSource), nameof(_RuntimeConstantSource.Value));
    }

    [Fact]
    public virtual async Task Runtime_constant_named_like_an_unsafe_accessor()
    {
        var contextFactory = await InitializeNonSharedTest<AccessorNameContext>();

        await Test(
            """
await using var context = new AdHocPrecompiledQueryRelationalTestBase.AccessorNameContext(dbContextOptions);
var entities = await context.Entities.ToListAsync();
""",
            typeof(AccessorNameContext),
            contextFactory.GetOptions(),
            interceptorCodeAsserter: code => Assert.Contains(
                "UnsafeAccessor_Microsoft_EntityFrameworkCore_Query_AccessorNameEntity_set_NameBytes(", code));
    }

    // The model alone can make a runtime constant take a generated name: a JSON property name is emitted as a runtime constant with
    // a Bytes suffix, and a private setter that materialization goes through is reached by an unsafe accessor named after its type
    // and the setter method.
    public class AccessorNameContext(DbContextOptions options) : DbContext(options)
    {
        public DbSet<AccessorNameEntity> Entities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<AccessorNameEntity>(
                b =>
                {
                    b.Property(x => x.NameBytes).UsePropertyAccessMode(PropertyAccessMode.Property);
                    b.ComplexProperty(
                        x => x.Nested, nb =>
                        {
                            nb.ToJson();
                            nb.Property(x => x.Name)
                                .HasJsonPropertyName("UnsafeAccessor_Microsoft_EntityFrameworkCore_Query_AccessorNameEntity_set_Name");
                        });
                });
    }

    public class AccessorNameEntity
    {
        public Guid Id { get; set; }
        public byte[]? NameBytes { get; private set; }
        public AccessorNameNested Nested { get; set; } = new();
    }

    public class AccessorNameNested
    {
        public string Name { get; set; } = "";
    }
#pragma warning restore EF9100

    #endregion

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

    protected override string NonSharedStoreName
        => "AdHocPrecompiledQueryTest";
}

// Outside the test class so that the generated code refers to it by its simple name, which a runtime constant field can take
public static class _RuntimeConstantSource
{
    public static byte[] Value
        => [1];
}
