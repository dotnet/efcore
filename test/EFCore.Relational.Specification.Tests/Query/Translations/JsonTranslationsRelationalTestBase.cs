// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Nodes;

namespace Microsoft.EntityFrameworkCore.Query.Translations;

// This test suite covers translations of JSON functions on EF.Functions (e.g. EF.Functions.JsonExists).
// It does not cover general, built-in JSON support via complex type mapping, etc.
public abstract class JsonTranslationsRelationalTestBase<TFixture>(TFixture fixture) : QueryTestBase<TFixture>(fixture)
    where TFixture : JsonTranslationsRelationalTestBase<TFixture>.JsonTranslationsQueryFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task JsonExists_on_scalar_string_column()
        => AssertQuery(
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => EF.Functions.JsonExists(b.JsonString, "$.OptionalInt")),
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => ((IDictionary<string, JsonNode>)JsonNode.Parse(b.JsonString)!).ContainsKey("OptionalInt")));

    [ConditionalFact]
    public virtual Task JsonExists_on_complex_property()
        => AssertQuery(
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => EF.Functions.JsonExists(b.JsonComplexType, "$.OptionalInt")),
            ss => ss.Set<JsonTranslationsEntity>()
                .Where(b => ((IDictionary<string, JsonNode>)JsonNode.Parse(b.JsonString)!).ContainsKey("OptionalInt")));

    public class JsonTranslationsEntity
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public required string JsonString { get; set; }

        public required JsonComplexType JsonComplexType { get; set; }
    }

    public class JsonComplexType
    {
        public required int RequiredInt { get; set; }
        public int? OptionalInt { get; set; }
    }

    public class JsonTranslationsQueryContext(DbContextOptions options) : PoolableDbContext(options)
    {
        public DbSet<JsonTranslationsEntity> JsonEntities { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<JsonTranslationsEntity>().ComplexProperty(j => j.JsonComplexType, j => j.ToJson());
    }

    // The translation tests usually use BasicTypesQueryFixtureBase, which manages a single database with all the data needed for the tests.
    // However, here in the JSON translation tests we use a separate fixture and database, since not all providers necessary implement full
    // JSON support, and we don't want to make life difficult for them with the basic translation tests.
    public abstract class JsonTranslationsQueryFixtureBase : SharedStoreFixtureBase<JsonTranslationsQueryContext>, IQueryFixtureBase, ITestSqlLoggerFactory
    {
        private JsonTranslationsData? _expectedData;

        protected override string StoreName
            => "JsonTranslationsQueryTest";

        protected override async Task SeedAsync(JsonTranslationsQueryContext context)
        {
            var data = new JsonTranslationsData();
            context.AddRange(data.JsonTranslationsEntities);
            await context.SaveChangesAsync();

            var entityType = context.Model.FindEntityType(typeof(JsonTranslationsEntity))!;
            var sqlGenerationHelper = context.GetService<ISqlGenerationHelper>();
            var table = sqlGenerationHelper.DelimitIdentifier(entityType.GetTableName()!);
            var idColumn = sqlGenerationHelper.DelimitIdentifier(
                entityType.FindProperty(nameof(JsonTranslationsEntity.Id))!.GetColumnName());
            var complexTypeColumn = sqlGenerationHelper.DelimitIdentifier(
                entityType.FindComplexProperty(nameof(JsonTranslationsEntity.JsonComplexType))!.ComplexType.GetContainerColumnName()!);

            await context.Database.ExecuteSqlRawAsync(
                $$"""UPDATE {{table}} SET {{complexTypeColumn}} = {{RemoveJsonProperty(complexTypeColumn, "$.OptionalInt")}} WHERE {{idColumn}} = 4""");
        }

        protected abstract string RemoveJsonProperty(string column, string jsonPath);

        public virtual ISetSource GetExpectedData()
            => _expectedData ??= new JsonTranslationsData();

        public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object?, object?>>
        {
            { typeof(JsonTranslationsEntity), e => ((JsonTranslationsEntity?)e)?.Id },
        }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object?, object?>>
        {
            {
                typeof(JsonTranslationsEntity), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (JsonTranslationsEntity)e!;
                        var aa = (JsonTranslationsEntity)a;

                        Assert.Equal(ee.Id, aa.Id);

                        Assert.Equal(ee.JsonString, aa.JsonString);
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);

        public Func<DbContext> GetContextCreator()
            => CreateContext;

        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;
    }

    public class JsonTranslationsData : ISetSource
    {
        public IReadOnlyList<JsonTranslationsEntity> JsonTranslationsEntities { get; } = CreateJsonTranslationsEntities();

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
            => typeof(TEntity) == typeof(JsonTranslationsEntity)
                ? (IQueryable<TEntity>)JsonTranslationsEntities.AsQueryable()
                : throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));

        public static IReadOnlyList<JsonTranslationsEntity> CreateJsonTranslationsEntities() =>
        [
            // In the following, JsonString should correspond exactly to JsonComplexType; we don't currently support mapping both
            // a string scalar property and a complex JSON property to the same column in the database.

            new()
            {
                Id = 1,
                JsonString = """{ "RequiredInt": 8, "OptionalInt": 8 }""",
                JsonComplexType = new()
                {
                    RequiredInt = 8,
                    OptionalInt = 8
                }
            },
            // Different values
            new()
            {
                Id = 2,
                JsonString = """{ "RequiredInt": 9, "OptionalInt": 9 }""",
                JsonComplexType = new()
                {
                    RequiredInt = 9,
                    OptionalInt = 9
                }
            },
            // OptionalInt is null.
            new()
            {
                Id = 3,
                JsonString = """{ "RequiredInt": 10, "OptionalInt": null }""",
                JsonComplexType = new()
                {
                    RequiredInt = 10,
                    OptionalInt = null
                }
            },
            // OptionalInt is missing (not null).
            // Note that this requires a manual SQL update since EF's complex type support always writes out the property (with null);
            // any change here requires updating JsonTranslationsQueryContext.SeedAsync as well.
            new()
            {
                Id = 4,
                JsonString = """{ "RequiredInt": 10 }""",
                JsonComplexType = new()
                {
                    RequiredInt = 10,
                    OptionalInt = null // This will be replaced by a missing property
                }
            }
        ];
    }

    protected JsonTranslationsQueryContext CreateContext()
        => Fixture.CreateContext();
}
