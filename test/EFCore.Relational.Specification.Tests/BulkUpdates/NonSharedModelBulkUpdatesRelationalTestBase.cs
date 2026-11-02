// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using System.Linq.Expressions;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace Microsoft.EntityFrameworkCore.BulkUpdates;

public abstract class NonSharedModelBulkUpdatesRelationalTestBase(NonSharedFixture fixture) : NonSharedModelBulkUpdatesTestBase(fixture)
{
    protected override string NonSharedStoreName
        => "NonSharedModelBulkUpdatesTests";

    [Theory, InlineData(false, false), InlineData(true, false), InlineData(false, true), InlineData(true, true)]
    public virtual async Task Update_JSON_scalar_with_custom_reader_writer(bool async, bool typeMappingDefault)
    {
        var factory = await InitializeNonSharedTest<PoolableDbContext>(
            onModelCreating: b =>
            {
                var entity = b.Entity<JsonWriterItem>().ToTable("JsonWriterItems");
                entity.Property(e => e.Id).ValueGeneratedNever();
                var data = entity.ComplexProperty(e => e.Data).ToJson();
                if (!typeMappingDefault)
                {
                    data.Property(e => e.Number).HasJsonValueReaderWriter<IntAsStringReaderWriter>();
                    data.Property(e => e.Timestamp).HasJsonValueReaderWriter<UnixDateTimeReaderWriter>();
                    data.Property(e => e.Enabled).HasJsonValueReaderWriter<BoolAsStringReaderWriter>();
                }
            },
            configureConventions: c =>
            {
                if (typeMappingDefault)
                {
                    c.DefaultTypeMapping<int>().HasJsonValueReaderWriter<IntAsStringReaderWriter>();
                    c.DefaultTypeMapping<DateTime>().HasJsonValueReaderWriter<UnixDateTimeReaderWriter>();
                    c.DefaultTypeMapping<bool>().HasJsonValueReaderWriter<BoolAsStringReaderWriter>();
                }
            },
            seed: context =>
            {
                context.Add(
                    new JsonWriterItem
                    {
                        Id = 1, NativeNumber = 7,
                        Data = new JsonWriterData { Number = 1, Timestamp = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
                    });
                return context.SaveChangesAsync();
            });
        await using var context = factory.CreateDbContext();
        await context.Database.OpenConnectionAsync();
        var query = context.Set<JsonWriterItem>();
        var expectedEnabled = false;
        Assert.Equal(
            1, async
                ? await query.ExecuteUpdateAsync(s => s.SetProperty(e => e.Data.Number, 42))
                : query.ExecuteUpdate(s => s.SetProperty(e => e.Data.Number, 42)));
        await AssertData(42, new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var timestamp = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);
        Assert.Equal(
            1, async
                ? await query.ExecuteUpdateAsync(s => s.SetProperty(e => e.Data.Timestamp, timestamp))
                : query.ExecuteUpdate(s => s.SetProperty(e => e.Data.Timestamp, timestamp)));
        await AssertData(42, timestamp);

        Assert.Equal(
            1, async
                ? await query.ExecuteUpdateAsync(s => s.SetProperty(e => e.Data.Enabled, false))
                : query.ExecuteUpdate(s => s.SetProperty(e => e.Data.Enabled, false)));
        await AssertData(42, timestamp);
        var enabled = true;
        Assert.Equal(
            1, async
                ? await query.ExecuteUpdateAsync(s => s.SetProperty(e => e.Data.Enabled, enabled))
                : query.ExecuteUpdate(s => s.SetProperty(e => e.Data.Enabled, enabled)));
        expectedEnabled = enabled;
        await AssertData(42, timestamp);

        for (var number = 43; number <= 44; number++)
        {
            ClearLog();
            Assert.Equal(
                1, async
                    ? await query.ExecuteUpdateAsync(s => s.SetProperty(e => e.Data.Number, number))
                    : query.ExecuteUpdate(s => s.SetProperty(e => e.Data.Number, number)));
            await AssertData(number, timestamp);
        }

        await AssertTranslationFailedWithDetails(
            RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToArbitraryExpression,
            async () =>
            {
                if (async)
                {
                    await query.ExecuteUpdateAsync(s => s.SetProperty(e => e.Data.Number, e => e.Data.Number + 1));
                }
                else
                {
                    query.ExecuteUpdate(s => s.SetProperty(e => e.Data.Number, e => e.Data.Number + 1));
                }
            });
        await AssertTranslationFailedWithDetails(
            RelationalStrings.ExecuteUpdateCannotSetJsonPropertyToNonJsonColumn,
            async () =>
            {
                if (async)
                {
                    await query.ExecuteUpdateAsync(s => s.SetProperty(e => e.Data.Number, e => e.NativeNumber));
                }
                else
                {
                    query.ExecuteUpdate(s => s.SetProperty(e => e.Data.Number, e => e.NativeNumber));
                }
            });

        async Task AssertData(int expectedNumber, DateTime expectedTimestamp)
        {
            var item = async ? await query.AsNoTracking().SingleAsync() : query.AsNoTracking().Single();
            Assert.Equal(expectedNumber, item.Data.Number);
            Assert.Equal(expectedTimestamp, item.Data.Timestamp);
            Assert.Equal(expectedEnabled, item.Data.Enabled);
            Assert.Equal(7, item.NativeNumber);
            await using var command = context.Database.GetDbConnection().CreateCommand();
            command.CommandText = ((RelationalTestStore)NonSharedTestStore)
                .NormalizeDelimitersInRawString("SELECT [Data] FROM [JsonWriterItems]");
            var json = async ? await command.ExecuteScalarAsync() : command.ExecuteScalar();
            using var document = JsonDocument.Parse(Assert.IsType<string>(json));
            Assert.Equal(expectedNumber.ToString(CultureInfo.InvariantCulture), document.RootElement.GetProperty("Number").GetString());
            Assert.Equal(expectedEnabled ? "yes" : "no", document.RootElement.GetProperty("Enabled").GetString());
            Assert.Equal(
                new DateTimeOffset(expectedTimestamp).ToUnixTimeMilliseconds(),
                document.RootElement.GetProperty("Timestamp").GetInt64());
        }
    }

    protected class JsonWriterItem
    {
        public int Id { get; set; }
        public int NativeNumber { get; set; }
        public JsonWriterData Data { get; set; } = null!;
    }

    protected class JsonWriterData
    {
        public int Number { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Enabled { get; set; }
    }

    public sealed class BoolAsStringReaderWriter : JsonValueReaderWriter<bool>
    {
        public static BoolAsStringReaderWriter Instance { get; } = new();
        public override bool FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetString() == "yes";
        public override void ToJsonTyped(Utf8JsonWriter writer, bool value)
            => writer.WriteStringValue(value ? "yes" : "no");
        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(BoolAsStringReaderWriter), nameof(Instance));
    }

    public sealed class IntAsStringReaderWriter : JsonValueReaderWriter<int>
    {
        public static IntAsStringReaderWriter Instance { get; } = new();
        public override int FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => int.Parse(manager.CurrentReader.GetString()!, CultureInfo.InvariantCulture);
        public override void ToJsonTyped(Utf8JsonWriter writer, int value)
            => writer.WriteStringValue(value.ToString(CultureInfo.InvariantCulture));
        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(IntAsStringReaderWriter), nameof(Instance));
    }

    public sealed class UnixDateTimeReaderWriter : JsonValueReaderWriter<DateTime>
    {
        public static UnixDateTimeReaderWriter Instance { get; } = new();
        public override DateTime FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => DateTimeOffset.FromUnixTimeMilliseconds(manager.CurrentReader.GetInt64()).UtcDateTime;
        public override void ToJsonTyped(Utf8JsonWriter writer, DateTime value)
            => writer.WriteNumberValue(new DateTimeOffset(value).ToUnixTimeMilliseconds());
        public override Expression ConstructorExpression
            => Expression.Property(null, typeof(UnixDateTimeReaderWriter), nameof(Instance));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Delete_aggregate_root_when_table_sharing_with_non_owned_throws(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context28671>(
            onModelCreating: mb =>
            {
                mb.Entity<Owner>().HasOne<OtherReference>().WithOne().HasForeignKey<OtherReference>(e => e.Id);
                mb.Entity<OtherReference>().ToTable(nameof(Owner));
            });

        await AssertTranslationFailedWithDetails(
            RelationalStrings.ExecuteDeleteOnTableSplitting(nameof(Owner)),
            () => AssertDelete(
                async, contextFactory.CreateDbContext,
                context => context.Set<Owner>(), rowsAffectedCount: 0));
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_main_table_in_entity_with_entity_splitting(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<Blog>()
                .ToTable("Blogs")
                .SplitToTable(
                    "BlogsPart1", tb =>
                    {
                        tb.Property(b => b.Title);
                        tb.Property(b => b.Rating);
                    }),
            seed: async context =>
            {
                context.Set<Blog>().Add(new Blog { Title = "SomeBlog" });
                await context.SaveChangesAsync();
            });

        await AssertUpdate(
            async,
            contextFactory.CreateDbContext,
            ss => ss.Set<Blog>(),
            s => s.SetProperty(b => b.CreationTimestamp, b => new DateTime(2020, 1, 1)),
            rowsAffectedCount: 1);
    }

    [Theory, MemberData(nameof(IsAsyncData))]
    public virtual async Task Update_non_main_table_in_entity_with_entity_splitting(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<DbContext>(
            onModelCreating: mb => mb.Entity<Blog>()
                .ToTable("Blogs")
                .SplitToTable(
                    "BlogsPart1", tb =>
                    {
                        tb.Property(b => b.Title);
                        tb.Property(b => b.Rating);
                    }),
            seed: async context =>
            {
                context.Set<Blog>().Add(new Blog { Title = "SomeBlog" });
                await context.SaveChangesAsync();
            });

        await AssertUpdate(
            async,
            contextFactory.CreateDbContext,
            ss => ss.Set<Blog>(),
            s => s
                .SetProperty(b => b.Title, b => b.Rating.ToString())
                .SetProperty(b => b.Rating, b => b.Title!.Length),
            rowsAffectedCount: 1);
    }

    [Theory, MemberData(nameof(IsAsyncData))] // #34677
    public virtual async Task Delete_with_view_mapping(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context34677>(seed: async context => await context.Seed());

        await AssertDelete(
            async,
            contextFactory.CreateDbContext,
            ss => ss.Foos,
            rowsAffectedCount: 1);
    }

    [Theory, MemberData(nameof(IsAsyncData))] // #34677
    public virtual async Task Update_with_view_mapping(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context34677>(seed: async context => await context.Seed());

        await AssertUpdate(
            async,
            contextFactory.CreateDbContext,
            ss => ss.Foos,
            s => s.SetProperty(f => f.Data, "Updated"),
            rowsAffectedCount: 1);
    }

    [Theory, MemberData(nameof(IsAsyncData))] // #34677
    public virtual async Task Update_complex_type_with_view_mapping(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context34677>(seed: async context => await context.Seed());

        await AssertUpdate(
            async,
            contextFactory.CreateDbContext,
            ss => ss.Foos,
            s => s.SetProperty(f => f.ComplexThing, new Context34677.ComplexThing { Prop1 = 3, Prop2 = 4 }),
            rowsAffectedCount: 1);
    }

    [Theory, MemberData(nameof(IsAsyncData))] // #34677
    public virtual async Task Update_complex_type_property_with_view_mapping(bool async)
    {
        var contextFactory = await InitializeNonSharedTest<Context34677>(seed: async context => await context.Seed());

        await AssertUpdate(
            async,
            contextFactory.CreateDbContext,
            ss => ss.Foos,
            s => s.SetProperty(f => f.ComplexThing.Prop1, 6),
            rowsAffectedCount: 1);
    }

    protected class Context34677(DbContextOptions options) : DbContext(options)
    {
        public DbSet<Foo> Foos
            => Set<Foo>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
            => modelBuilder.Entity<Foo>(eb => eb
                .ToTable("Blogs")
                .ToView("BlogsView")
                .ComplexProperty(b => b.ComplexThing).IsRequired());

        public async Task Seed()
        {
            Add(
                new Foo
                {
                    Id = 1,
                    Data = "Data",
                    ComplexThing = new ComplexThing { Prop1 = 1, Prop2 = 2 }
                });
            await SaveChangesAsync();
        }

        public class Foo
        {
            [DatabaseGenerated(DatabaseGeneratedOption.None)]
            public int Id { get; set; }

            public string? Data { get; set; }
            public ComplexThing ComplexThing { get; set; } = null!;
        }

        public class ComplexThing
        {
            public int Prop1 { get; set; }
            public int Prop2 { get; set; }
        }
    }

    #region HelperMethods

    protected static async Task AssertTranslationFailedWithDetails(string details, Func<Task> query)
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(query);
        Assert.StartsWith(CoreStrings.NonQueryTranslationFailed("")[..^1], exception.Message);
        var innerException = Assert.IsType<InvalidOperationException>(exception.InnerException);
        Assert.Equal(details, innerException.Message);
    }

    public override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected TestSqlLoggerFactory TestSqlLoggerFactory
        => (TestSqlLoggerFactory)ListLoggerFactory;

    protected void ClearLog()
        => TestSqlLoggerFactory.Clear();

    #endregion
}
