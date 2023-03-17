// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Query;

public class PrimitiveCollectionsQueryTestBase<TFixture> : QueryTestBase<TFixture>
    where TFixture : PrimitiveCollectionsQueryTestBase<TFixture>.PrimitiveCollectionsQueryFixtureBase, new()
{
    protected PrimitiveCollectionsQueryTestBase(TFixture fixture)
        : base(fixture)
    {
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_of_ints_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 10, 999 }.Contains(c.Int)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_of_nullable_ints_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new int?[] { 10, 999 }.Contains(c.NullableInt)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_of_nullable_ints_Contains_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new int?[] { null, 999 }.Contains(c.NullableInt)),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_zero_values(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once UseArrayEmptyMethod
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new int[0].Count(i => i > c.Id) == 1),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_one_value(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2 }.Count(i => i > c.Id) == 1),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_two_values(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999 }.Count(i => i > c.Id) == 1),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Count_with_three_values(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999, 1000 }.Count(i => i > c.Id) == 2),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_zero_values(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once UseArrayEmptyMethod
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new int[0].Contains(c.Id)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_one_value(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2 }.Contains(c.Id)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_two_values(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999 }.Contains(c.Id)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_three_values(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 2, 999, 1000 }.Contains(c.Id)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Contains_with_all_parameters(bool async)
    {
        var (i, j) = (2, 999);

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { i, j }.Contains(c.Id)),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Inline_collection_Contains_with_parameter_and_column_based_expression(bool async)
    {
        var i = 2;

        await AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { i, c.Int }.Contains(c.Id)),
                entryCount: 1));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_Count(bool async)
    {
        var ids = new[] { 2, 999 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ids.Count(i => i > c.Id) == 1),
            entryCount: 2);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_ints_Contains(bool async)
    {
        var ints = new[] { 10, 999 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Contains(c.Int)),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_nullable_ints_Contains(bool async)
    {
        var nullableInts = new int?[] { 10, 999 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => nullableInts.Contains(c.NullableInt)),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_nullable_ints_Contains_null(bool async)
    {
        var nullableInts = new int?[] { null, 999 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => nullableInts.Contains(c.NullableInt)),
            entryCount: 2);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_strings_Contains(bool async)
    {
        var strings = new[] { "10", "999" };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => strings.Contains(c.String)),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_DateTimes_Contains(bool async)
    {
        var dateTimes = new[]
        {
            new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc),
            new DateTime(9999, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => dateTimes.Contains(c.DateTime)),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_bools_Contains(bool async)
    {
        var bools = new[] { true };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => bools.Contains(c.Bool)),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_of_enums_Contains(bool async)
    {
        var enums = new[] { MyEnum.Value1, MyEnum.Value4 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => enums.Contains(c.Enum)),
            entryCount: 2);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_null_Contains(bool async)
    {
        int[] ints = null;

        await AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Contains(c.Int)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => false));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_of_ints_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Contains(10)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_of_nullable_ints_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableInts.Contains(10)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_of_nullable_ints_Contains_null(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.NullableInts.Contains(null)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_of_bools_Contains(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Bools.Contains(true)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Count_method(bool async)
        => AssertQuery(
            async,
            // ReSharper disable once UseCollectionCountProperty
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Count() == 2),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Length(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Length == 2),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_index_int(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints[1] == 10),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Ints.Length >= 2 ? c.Ints[1] : -1) == 10),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_index_string(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Strings[1] == "10"),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Strings.Length >= 2 ? c.Strings[1] : "-1") == "10"),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_index_datetime(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => c.DateTimes[1] == new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc)),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => (c.DateTimes.Length >= 2 ? c.DateTimes[1] : default) == new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_index_beyond_end(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints[999] == 10),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => false),
            entryCount: 0);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_index_Column(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => new[] { 1, 2, 3 }[c.Int] == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Int <= 2 ? new[] { 1, 2, 3 }[c.Int] : -1) == 1),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_index_Column(bool async)
    {
        var ints = new[] { 1, 2, 3 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints[c.Int] == 1),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Int <= 2 ? ints[c.Int] : -1) == 1),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_ElementAt(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.ElementAt(1) == 10),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => (c.Ints.Length >= 2 ? c.Ints.ElementAt(1) : -1) == 10),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Skip(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Skip(1).Count() == 2),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Take(2).Contains(11)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Skip_Take(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Skip(1).Take(2).Contains(11)),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Any(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Any()),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_projection_from_top_level(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().OrderBy(c => c.Id).Select(c => c.Ints),
            elementAsserter: (a, b) => Assert.Equivalent(a, b),
            assertOrder: true);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_and_parameter_collection_Join(bool async)
    {
        var ints = new[] { 11, 111 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Join(ints, i => i, j => j, (i, j) => new { I = i, J = j }).Count() == 2),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Parameter_collection_Concat_column_collection(bool async)
    {
        var ints = new[] { 11, 111 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => ints.Concat(c.Ints).Count() == 2),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Union_parameter_collection(bool async)
    {
        var ints = new[] { 11, 111 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Union(ints).Count() == 2),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_Intersect_inline_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Intersect(new[] { 11, 111 }).Count() == 2),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Inline_collection_Except_column_collection(bool async)
        // Note that since the VALUES is on the left side of the set operation, it must assign column names, otherwise the column coming
        // out of the set operation has undetermined naming.
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(
                c => new[] { 11, 111 }.Except(c.Ints).Count(i => i % 2 == 1) == 2),
            entryCount: 2);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_equality_parameter_collection(bool async)
    {
        var ints = new[] { 1, 10 };

        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints == ints),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.SequenceEqual(ints)),
            entryCount: 1);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Column_collection_Concat_parameter_collection_equality_inline_collection_not_supported(bool async)
    {
        var ints = new[] { 1, 10 };

        await AssertTranslationFailed(
            () => AssertQuery(
                async,
                ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Concat(ints) == new[] { 1, 11, 111, 1, 10 })));
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_equality_inline_collection(bool async)
        => AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints == new[] { 1, 10 }),
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.SequenceEqual(new[] { 1, 10 })),
            entryCount: 1);

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_in_subquery_Count_as_compiled_query(bool async)
    {
        // The Skip causes a pushdown into a subquery before the Union, and so the projection on the left side of the union points to the
        // subquery as its table, and not directly to the parameter's table.
        // This creates an initially untyped ColumnExpression referencing the pushed-down subquery; it must also be inferred.
        // Note that this must be a compiled query, since with normal queries the Skip(1) gets client-evaluated.
        // TODO:
        var compiledQuery = EF.CompileQuery(
            (PrimitiveCollectionsContext context, int[] ints)
                => context.Set<PrimitiveCollectionsEntity>().Where(p => ints.Skip(1).Count(i => i > p.Id) == 1).Count());

        await using var context = Fixture.CreateContext();
        var ints = new[] { 10, 111 };

        // TODO: Complete
        var results = compiledQuery(context, ints);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual async Task Parameter_collection_in_subquery_Union_column_collection_as_compiled_query(bool async)
    {
        // The Skip causes a pushdown into a subquery before the Union, and so the projection on the left side of the union points to the
        // subquery as its table, and not directly to the parameter's table.
        // This creates an initially untyped ColumnExpression referencing the pushed-down subquery; it must also be inferred.
        // Note that this must be a compiled query, since with normal queries the Skip(1) gets client-evaluated.
        var compiledQuery = EF.CompileQuery(
            (PrimitiveCollectionsContext context, int[] ints)
                => context.Set<PrimitiveCollectionsEntity>().Where(p => ints.Skip(1).Union(p.Ints).Count() == 3));

        await using var context = Fixture.CreateContext();
        var ints = new[] { 10, 111 };

        // TODO: Complete
        var results = compiledQuery(context, ints).ToList();
    }

    [ConditionalFact]
    public virtual void Parameter_collection_in_subquery_and_Convert_as_compiled_query()
    {
        // The array indexing is translated as a subquery over e.g. OpenJson with LIMIT/OFFSET.
        // Since there's a CAST over that, the type mapping inference from the other side (p.String) doesn't propagate inside to the
        // subquery. In this case, the CAST operand gets the default CLR type mapping, but that's object in this case.
        // We should apply the default type mapping to the parameter, but need to figure out the exact rules when to do this.
        var query = EF.CompileQuery(
            (PrimitiveCollectionsContext context, object[] parameters)
                => context.Set<PrimitiveCollectionsEntity>().Where(p => p.String == (string)parameters[0]));

        using var context = Fixture.CreateContext();

        var exception = Assert.Throws<InvalidOperationException>(() => query(context, new[] { "foo" }).ToList());

        Assert.Contains("in the SQL tree does not have a type mapping assigned", exception.Message);
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public virtual Task Column_collection_in_subquery_Union_parameter_collection(bool async)
    {
        var ints = new[] { 10, 111 };

        // The Skip causes a pushdown into a subquery before the Union. This creates an initially untyped ColumnExpression referencing the
        // pushed-down subquery; it must also be inferred
        return AssertQuery(
            async,
            ss => ss.Set<PrimitiveCollectionsEntity>().Where(c => c.Ints.Skip(1).Union(ints).Count() == 3),
            entryCount: 1);
    }

    public abstract class PrimitiveCollectionsQueryFixtureBase : SharedStoreFixtureBase<PrimitiveCollectionsContext>, IQueryFixtureBase
    {
        private PrimitiveArrayData _expectedData;

        protected override string StoreName
            => "PrimitiveCollectionsTest";

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c.Log(CoreEventId.DistinctAfterOrderByWithoutRowLimitingOperatorWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            => modelBuilder.Entity<PrimitiveCollectionsEntity>().Property(p => p.Id).ValueGeneratedNever();

        protected override void Seed(PrimitiveCollectionsContext context)
            => new PrimitiveArrayData(context);

        public virtual ISetSource GetExpectedData()
            => _expectedData ??= new PrimitiveArrayData();

        public IReadOnlyDictionary<Type, object> EntitySorters { get; } = new Dictionary<Type, Func<object, object>>
        {
            { typeof(PrimitiveCollectionsEntity), e => ((PrimitiveCollectionsEntity)e)?.Id }
        }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> EntityAsserters { get; } = new Dictionary<Type, Action<object, object>>
        {
            {
                typeof(PrimitiveCollectionsEntity), (e, a) =>
                {
                    Assert.Equal(e == null, a == null);

                    if (a != null)
                    {
                        var ee = (PrimitiveCollectionsEntity)e;
                        var aa = (PrimitiveCollectionsEntity)a;

                        Assert.Equal(ee.Id, aa.Id);
                        Assert.Equivalent(ee.Ints, aa.Ints, strict: true);
                        Assert.Equivalent(ee.Strings, aa.Strings, strict: true);
                        Assert.Equivalent(ee.DateTimes, aa.DateTimes, strict: true);
                        Assert.Equivalent(ee.Bools, aa.Bools, strict: true);
                        // TODO: Complete
                    }
                }
            }
        }.ToDictionary(e => e.Key, e => (object)e.Value);
    }

    public class PrimitiveCollectionsContext : PoolableDbContext
    {
        public PrimitiveCollectionsContext(DbContextOptions options)
            : base(options)
        {
        }
    }

    public class PrimitiveCollectionsEntity
    {
        public int Id { get; set; }

        public string String { get; set; }
        public int Int { get; set; }
        public DateTime DateTime { get; set; }
        public bool Bool { get; set; }
        public MyEnum Enum { get; set; }
        public int? NullableInt { get; set; }

        public string[] Strings { get; set; }
        public int[] Ints { get; set; }
        public DateTime[] DateTimes { get; set; }
        public bool[] Bools { get; set; }
        public MyEnum[] Enums { get; set; }
        public int?[] NullableInts { get; set; }
    }

    public enum MyEnum { Value1, Value2, Value3, Value4 }

    public class PrimitiveArrayData : ISetSource
    {
        public IReadOnlyList<PrimitiveCollectionsEntity> PrimitiveArrayEntities { get; }

        public PrimitiveArrayData(PrimitiveCollectionsContext context = null)
        {
            PrimitiveArrayEntities = CreatePrimitiveArrayEntities();

            if (context != null)
            {
                context.AddRange(PrimitiveArrayEntities);
                context.SaveChanges();
            }
        }

        public IQueryable<TEntity> Set<TEntity>()
            where TEntity : class
        {
            if (typeof(TEntity) == typeof(PrimitiveCollectionsEntity))
            {
                return (IQueryable<TEntity>)PrimitiveArrayEntities.AsQueryable();
            }

            throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
        }

        private static IReadOnlyList<PrimitiveCollectionsEntity> CreatePrimitiveArrayEntities()
            => new List<PrimitiveCollectionsEntity>
            {
                new()
                {
                    Id = 1,

                    Int = 10,
                    String = "10",
                    DateTime = new DateTime(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc),
                    Bool = true,
                    Enum = MyEnum.Value1,
                    NullableInt = 10,

                    Ints = new[] { 1, 10 },
                    Strings = new[] { "1", "10" },
                    DateTimes = new DateTime[]
                    {
                        new(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new(2020, 1, 10, 12, 30, 0, DateTimeKind.Utc)
                    },
                    Bools = new[] { true, false },
                    Enums = new[] { MyEnum.Value1, MyEnum.Value2 },
                    NullableInts = new int?[] { 1, 10 },
                },
                new()
                {
                    Id = 2,

                    Int = 11,
                    String = "11",
                    DateTime = new DateTime(2020, 1, 11, 12, 30, 0, DateTimeKind.Utc),
                    Bool = false,
                    Enum = MyEnum.Value2,
                    NullableInt = null,

                    Ints = new[] { 1, 11, 111 },
                    Strings = new[] { "1", "11", "111" },
                    DateTimes = new DateTime[]
                    {
                        new(2020, 1, 1, 12, 30, 0, DateTimeKind.Utc),
                        new(2020, 1, 11, 12, 30, 0, DateTimeKind.Utc),
                        new(2020, 1, 31, 12, 30, 0, DateTimeKind.Utc)
                    },
                    Bools = new[] { false },
                    Enums = new[] { MyEnum.Value2, MyEnum.Value3 },
                    NullableInts = new int?[] { 1, 11, null },
                },
                new()
                {
                    Id = 3,

                    Int = 0,
                    String = "",
                    DateTime = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Bool = false,
                    Enum = MyEnum.Value1,
                    NullableInt = null,

                    Ints = Array.Empty<int>(),
                    Strings = Array.Empty<string>(),
                    DateTimes = Array.Empty<DateTime>(),
                    Bools = Array.Empty<bool>(),
                    Enums = Array.Empty<MyEnum>(),
                    NullableInts = Array.Empty<int?>(),
                }
            };
    }
}
